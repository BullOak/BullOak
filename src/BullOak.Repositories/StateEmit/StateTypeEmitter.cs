namespace BullOak.Repositories.StateEmit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;

    internal class StateTypeEmitter
    {
        private static int emittedIndex = 0;

        public static Type EmitType(Type typeToMake, string nameForClass = null)
        {
            var modelBuilder = GetModelBuilder();

            if (!typeToMake.IsInterface)
                throw new ArgumentException($"Parameter must be type of an interface", nameof(typeToMake));
            if (typeToMake.GetMethods().Any(m => !m.IsHideBySig))
                throw new ArgumentException("Parameter must be of an interface type that does contain methods.",
                    nameof(typeToMake));

            return EmitType(modelBuilder, typeToMake, nameForClass);
        }

        public static Dictionary<Type, Type> EmitTypes(params Type[] typesToMake)
        {
            var modelBuilder = GetModelBuilder();

            if (typesToMake.Any(x => !x.IsInterface))
                throw new ArgumentException($"Parameter must be type of an interface", nameof(typesToMake));
            if (typesToMake.Any(x => x.GetMethods().Any(m => !m.IsHideBySig)))
                throw new ArgumentException("Parameter must be of an interface type that does contain methods.",
                    nameof(typesToMake));

            return EmitTypes(modelBuilder, typesToMake).ToDictionary(x => x.Item1, x => x.Item2);
        }

        private static IEnumerable<Tuple<Type, Type>> EmitTypes(ModuleBuilder modelBuilder, Type[] typesToMake)
            => typesToMake.Select(x => new Tuple<Type, Type>(x, EmitType(modelBuilder, x)));

        private static Type EmitType(ModuleBuilder modelBuilder, Type typeToMake, string nameToUseForType = null)
        {
            //var typeToMake = typeof(TState);
            if (!typeToMake.IsInterface)
                throw new ArgumentException($"Parameter must be type of an interface", nameof(typeToMake));
            if (typeToMake.GetMethods().Any(x => !x.IsHideBySig))
                throw new ArgumentException("Parameter must be of an interface type that does contain methods.",
                    nameof(typeToMake));

            var typeBuilder = modelBuilder.DefineType("StateGen_" + nameToUseForType ?? typeToMake.Name,
                TypeAttributes.NotPublic | TypeAttributes.Class);
            typeBuilder.AddInterfaceImplementation(typeof(ICanSwitchBackAndToReadOnly));
            typeBuilder.AddInterfaceImplementation(typeToMake);

            var canEditField = AddCanEditFieldAndProp(typeBuilder);

            foreach (var prop in typeToMake.GetProperties())
            {
                EmitProperty(prop, canEditField, modelBuilder, typeBuilder);
            }

            return typeBuilder.CreateType();
        }

        private static ModuleBuilder GetModelBuilder()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            assemblyName.Name += ".Emitter.Group" + Interlocked.Increment(ref emittedIndex);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }

        private static FieldBuilder AddCanEditFieldAndProp(TypeBuilder typeBuilder)
        {
            var canEditField =
                typeBuilder.DefineField("canEdit", typeof(bool), FieldAttributes.Private | FieldAttributes.HasDefault);

            typeBuilder.DefineProperty("CanEdit", PropertyAttributes.None, typeof(void),
                new[] {typeof(bool)});

            var setMethodBuilder = typeBuilder.DefineMethod("set_CanEdit", MethodAttributes.Public
                                                                           | MethodAttributes.Final
                                                                           | MethodAttributes.HideBySig
                                                                           | MethodAttributes.Virtual
                                                                           | MethodAttributes.NewSlot
                                                                           | MethodAttributes.SpecialName,
                CallingConventions.HasThis, typeof(void), new[] {typeof(bool)});

            var setMethodGenerator = setMethodBuilder.GetILGenerator();
            setMethodGenerator.Emit(OpCodes.Ldarg_0);
            setMethodGenerator.Emit(OpCodes.Ldarg_1);
            setMethodGenerator.Emit(OpCodes.Stfld, canEditField);
            setMethodGenerator.Emit(OpCodes.Ret);
            return canEditField;
        }

        private static void EmitProperty(PropertyInfo prop, FieldBuilder canEdit, ModuleBuilder moduleBuilder, TypeBuilder typeBuilder)
        {
            var propertyBuilder = typeBuilder.DefineProperty(prop.Name, PropertyAttributes.None, prop.PropertyType,
                new[] {prop.PropertyType});

            var field = typeBuilder.DefineField($"_{prop.Name}", prop.PropertyType, FieldAttributes.Private);
            var getMethodBuilder = typeBuilder.DefineMethod($"get_{prop.Name}", MethodAttributes.Public
                                                         | MethodAttributes.Final
                                                         | MethodAttributes.HideBySig
                                                         | MethodAttributes.Virtual
                                                         | MethodAttributes.NewSlot
                                                         | MethodAttributes.SpecialName,
                CallingConventions.HasThis, prop.PropertyType, null);
            var setMethodBuilder = typeBuilder.DefineMethod($"set_{prop.Name}", MethodAttributes.Public
                                                                                | MethodAttributes.Final
                                                                                | MethodAttributes.HideBySig
                                                                                | MethodAttributes.Virtual
                                                                                | MethodAttributes.NewSlot
                                                                                | MethodAttributes.SpecialName,
                CallingConventions.HasThis, typeof(void), new[] {prop.PropertyType});

            var getMethodGenerator = getMethodBuilder.GetILGenerator();
            getMethodGenerator.Emit(OpCodes.Ldarg_0);
            getMethodGenerator.Emit(OpCodes.Ldfld, field);
            getMethodGenerator.Emit(OpCodes.Ret);

            var setMethodGenerator = setMethodBuilder.GetILGenerator();
            var cannotEditLabel = setMethodGenerator.DefineLabel();
            setMethodGenerator.Emit(OpCodes.Ldarg_0);
            setMethodGenerator.Emit(OpCodes.Ldfld, canEdit);
            setMethodGenerator.Emit(OpCodes.Brfalse_S, cannotEditLabel);
            setMethodGenerator.Emit(OpCodes.Ldarg_0);
            setMethodGenerator.Emit(OpCodes.Ldarg_1);
            setMethodGenerator.Emit(OpCodes.Stfld, field);
            setMethodGenerator.Emit(OpCodes.Ret);
            setMethodGenerator.MarkLabel(cannotEditLabel);
            setMethodGenerator.Emit(OpCodes.Ldstr, "You can only edit this item during reconstitution");
            setMethodGenerator.ThrowException(typeof(Exception));
        }
    }
}
