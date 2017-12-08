namespace BullOak.Repositories.StateEmit
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class StateTypeEmitter
    {
        public static Type EmitType(Type typeOfState)
        {
            //var typeOfState = typeof(TState);
            if (!typeOfState.IsInterface)
                throw new ArgumentException($"Parameter must be type of an interface", nameof(typeOfState));
            if (typeOfState.GetMethods().Count(x => !x.IsHideBySig) > 0)
                throw new ArgumentException("Parameter must be of an interface type that does contain methods.", nameof(typeOfState));

            var modelBuilder = GetModelBuilder();
            var typeBuilder = modelBuilder.DefineType("StateGen_" + typeOfState.Name, TypeAttributes.NotPublic | TypeAttributes.Class);
            typeBuilder.AddInterfaceImplementation(typeof(ICanSwitchBackAndToReadOnly));
            typeBuilder.AddInterfaceImplementation(typeOfState);

            var canEditField = AddCanEditFieldAndProp(typeBuilder);

            foreach (var prop in typeOfState.GetProperties())
            {
                EmitProperty(prop, canEditField, modelBuilder, typeBuilder);
            }

            return typeBuilder.CreateType();
        }

        private static ModuleBuilder GetModelBuilder()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            //NOTE: If this changes, the InternalVisibleTo attribute in this assembly corresponding to this HAS to be changed
            // otherwise your clients will be seeing C# compiler errors
            assemblyName.Name += ".Emitter";
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
