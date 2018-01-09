namespace BullOak.Repositories.StateEmit.Emitters
{
    using System.Reflection;
    using System.Reflection.Emit;

    internal class OwnedStateClassEmitter : BaseClassEmitter
    {
        private FieldBuilder fieldToStoreValue;

        public sealed override void ClassSetup(TypeBuilder typeBuilder)
        { }
        public sealed override void EmitCtor(TypeBuilder typeBuilder)
        { }

        public sealed override void PropertySetup(TypeBuilder typeBuilder, PropertyInfo prop)
        {
            fieldToStoreValue = typeBuilder.DefineField($"_{prop.Name}", prop.PropertyType, FieldAttributes.Private);
        }

        public sealed override void EmitGetValueOpCodes(ILGenerator getMethodGenerator)
        {
            getMethodGenerator.Emit(OpCodes.Ldarg_0);
            getMethodGenerator.Emit(OpCodes.Ldfld, fieldToStoreValue);
            getMethodGenerator.Emit(OpCodes.Ret);
        }

        public sealed override void EmitSetValueOpCodes(ILGenerator setMethodGenerator)
        {
            setMethodGenerator.Emit(OpCodes.Ldarg_0);
            setMethodGenerator.Emit(OpCodes.Ldarg_1);
            setMethodGenerator.Emit(OpCodes.Stfld, fieldToStoreValue);
            setMethodGenerator.Emit(OpCodes.Ret);
        }
    }
}
