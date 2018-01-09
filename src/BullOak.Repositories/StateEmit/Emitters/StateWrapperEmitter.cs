namespace BullOak.Repositories.StateEmit.Emitters
{
    using System.Reflection;
    using System.Reflection.Emit;

    internal class StateWrapperEmitter : BaseClassEmitter
    {
        private FieldBuilder fieldToStoreValue;
        private ConstructorBuilder constructorBuilder;
        private PropertyInfo property;

        public sealed override void ClassSetup(TypeBuilder typeBuilder)
        {
            fieldToStoreValue = typeBuilder.DefineField($"_wrapped", InterfaceType, FieldAttributes.Private);
        }

        public sealed override void EmitCtor(TypeBuilder typeBuilder)
        {
            constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public
                                                     | MethodAttributes.HideBySig
                                                     | MethodAttributes.RTSpecialName
                                                     | MethodAttributes.SpecialName, CallingConventions.HasThis,
                new[] {InterfaceType});

            constructorBuilder.GetILGenerator()
                .ILEmit(OpCodes.Ldarg_0)
                .ILEmit(OpCodes.Ldarg_1)
                .ILEmit(OpCodes.Stfld, fieldToStoreValue)
                .ILEmit(OpCodes.Ret);
        }

        public sealed override void PropertySetup(TypeBuilder typeBuilder, PropertyInfo property)
        {
            this.property = property;
        }

        public sealed override void EmitGetValueOpCodes(ILGenerator getMethodGenerator)
        {
            getMethodGenerator.ILEmit(OpCodes.Ldarg_0)
                .ILEmit(OpCodes.Ldfld, fieldToStoreValue)
                .ILEmit(OpCodes.Callvirt, property.GetMethod)
                .ILEmit(OpCodes.Ret);
        }

        public sealed override void EmitSetValueOpCodes(ILGenerator setMethodGenerator)
        {
            setMethodGenerator.ILEmit(OpCodes.Ldarg_0)
                .ILEmit(OpCodes.Ldfld, fieldToStoreValue)
                .ILEmit(OpCodes.Ldarg_1)
                .ILEmit(OpCodes.Callvirt, property.SetMethod)
                .ILEmit(OpCodes.Ret);
        }
    }
}
