namespace BullOak.Repositories.StateEmit
{
    using System;

    [Serializable]
    internal class TypeCannotBeWrappedException : Exception
    {
        private Type type;

        public Type Type => type;

        public TypeCannotBeWrappedException(Type type)
            : base($"Wrapper for {type.Name} cannot be emitted because its not an interface.")
        {
            this.type = type;
        }
    }
}
