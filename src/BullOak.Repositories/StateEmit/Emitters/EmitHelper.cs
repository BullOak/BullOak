namespace BullOak.Repositories.StateEmit.Emitters
{
    using System.Reflection;
    using System.Reflection.Emit;

    public static class EmitHelper
    {
        public static ILGenerator ILEmit(this ILGenerator generator, OpCode code)
        {
            generator.Emit(code);
            return generator;
        }

        public static ILGenerator ILEmit(this ILGenerator generator, OpCode code, FieldInfo fieldInfo)
        {
            generator.Emit(code, fieldInfo);
            return generator;
        }

        public static ILGenerator ILEmit(this ILGenerator generator, OpCode code, MethodInfo fieldInfo)
        {
            generator.Emit(code, fieldInfo);
            return generator;
        }
    }
}
