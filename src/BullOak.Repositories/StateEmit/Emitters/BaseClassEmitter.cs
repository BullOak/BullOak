namespace BullOak.Repositories.StateEmit.Emitters
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;

    internal abstract class BaseClassEmitter
    {
        private Type interfaceType;

        protected Type InterfaceType
        {
            get => interfaceType;
            set => Interlocked.CompareExchange(ref interfaceType, value, null);
        }

        public virtual Type EmitType(ModuleBuilder modelBuilder, Type typeToMake, string nameToUseForType = null)
        {
            //var typeToMake = typeof(TState);
            if (!typeToMake.IsInterface)
                throw new ArgumentException($"Parameter must be type of an interface", nameof(typeToMake));
            if (typeToMake.GetMethods().Any(x => !x.IsHideBySig))
                throw new ArgumentException("Parameter must be of an interface type that does contain methods.",
                    nameof(typeToMake));

            InterfaceType = typeToMake;

            var typeBuilder = modelBuilder.DefineType("StateGen_" + (string.IsNullOrWhiteSpace(nameToUseForType) ? typeToMake.Name : nameToUseForType),
                TypeAttributes.Public | TypeAttributes.Class);
            typeBuilder.AddInterfaceImplementation(typeof(ICanSwitchBackAndToReadOnly));
            typeBuilder.AddInterfaceImplementation(typeToMake);

            ClassSetup(typeBuilder);
            EmitCtor(typeBuilder);

            var canEditField = AddCanEditFieldAndProp(typeBuilder);

            foreach (var prop in typeToMake.GetProperties())
            {
                EmitProperty(prop, canEditField, typeBuilder);
            }

            return typeBuilder.CreateType();
        }

        private FieldBuilder AddCanEditFieldAndProp(TypeBuilder typeBuilder)
        {
            var canEditField =
                typeBuilder.DefineField("canEdit", typeof(bool), FieldAttributes.Public | FieldAttributes.HasDefault);

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

        private void EmitProperty(PropertyInfo prop, FieldBuilder canEdit, TypeBuilder typeBuilder)
        {
            var propertyBuilder = typeBuilder.DefineProperty(prop.Name, PropertyAttributes.None, prop.PropertyType,
                new[] { prop.PropertyType });

            var getMethodBuilder = typeBuilder.DefineMethod($"get_{prop.Name}", MethodAttributes.Public
                                                         | MethodAttributes.Final
                                                         | MethodAttributes.HideBySig
                                                         | MethodAttributes.Virtual
                                                         | MethodAttributes.NewSlot
                                                         | MethodAttributes.SpecialName,
                CallingConventions.Standard, prop.PropertyType, null);
            var setMethodBuilder = typeBuilder.DefineMethod($"set_{prop.Name}", MethodAttributes.Public
                                                                                | MethodAttributes.Final
                                                                                | MethodAttributes.HideBySig
                                                                                | MethodAttributes.Virtual
                                                                                | MethodAttributes.NewSlot
                                                                                | MethodAttributes.SpecialName,
                CallingConventions.Standard, typeof(void), new[] { prop.PropertyType });

            PropertySetup(typeBuilder, prop);

            EmitGetValueOpCodes(getMethodBuilder.GetILGenerator());

            var setMethodGenerator = setMethodBuilder.GetILGenerator();
            var cannotEditLabel = setMethodGenerator.DefineLabel();
            setMethodGenerator.Emit(OpCodes.Ldarg_0);
            setMethodGenerator.Emit(OpCodes.Ldfld, canEdit);
            setMethodGenerator.Emit(OpCodes.Brfalse_S, cannotEditLabel);
            EmitSetValueOpCodes(setMethodGenerator);
            setMethodGenerator.MarkLabel(cannotEditLabel);
            setMethodGenerator.Emit(OpCodes.Ldstr, "You can only edit this item during reconstitution");
            setMethodGenerator.ThrowException(typeof(Exception));

            propertyBuilder.SetSetMethod(setMethodBuilder);
            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        public abstract void EmitCtor(TypeBuilder typeBuilder);
        public abstract void ClassSetup(TypeBuilder typeBuilder);
        public abstract void PropertySetup(TypeBuilder typeBuilder, PropertyInfo propInfo);
        public abstract void EmitGetValueOpCodes(ILGenerator getMethodGenerator);
        public abstract void EmitSetValueOpCodes(ILGenerator setMethodGenerator);
    }
}
