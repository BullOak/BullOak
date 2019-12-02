namespace BullOak.Repositories.Upconverting
{
    using System;

    [Serializable]
    internal class CannotInstantiateUpconverterException : Exception
    {
        public readonly Type upconverterType;

        public CannotInstantiateUpconverterException(string message, Type upconverterType)
            :base(message)
            => this.upconverterType = upconverterType;

        public CannotInstantiateUpconverterException(string message, Type upconverterType, Exception innerException)
            : base(message, innerException)
            => this.upconverterType = upconverterType;
    }
}
