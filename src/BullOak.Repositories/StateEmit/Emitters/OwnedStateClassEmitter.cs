namespace BullOak.Repositories.StateEmit.Emitters
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class OwnedStateClassEmitter : BaseClassEmitter
    {
        public override Type EmitType(ModuleBuilder modelBuilder, Type typeToMake, string nameToUseForType = null)
            => base.EmitType(modelBuilder, typeToMake, "OwneddStateEmitter_" + (string.IsNullOrWhiteSpace(
                                                                                       nameToUseForType)
                                                                                       ? typeToMake.Name
                                                                                       : nameToUseForType));

        private FieldBuilder fieldToStoreValue;

        public sealed override void ClassSetup(TypeBuilder typeBuilder)
        { }
        public sealed override void EmitCtor(TypeBuilder typeBuilder)
        { }

        public sealed override void PropertySetup(TypeBuilder typeBuilder, PropertyInfo propInfo)
        {
            fieldToStoreValue = typeBuilder.DefineField($"_{propInfo.Name}", propInfo.PropertyType,
                FieldAttributes.Private | FieldAttributes.HasDefault);
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
