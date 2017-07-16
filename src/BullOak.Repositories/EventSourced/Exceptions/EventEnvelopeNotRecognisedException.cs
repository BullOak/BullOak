namespace BullOak.Repositories.EventSourced.Exceptions
{
    using System;

    public class EventEnvelopeNotRecognisedException : Exception
    {
        public EventEnvelopeNotRecognisedException(Type expectedType, Type actualType)
            : base($"Expected type of envelope is {expectedType.AssemblyQualifiedName} but actual was {actualType.AssemblyQualifiedName}")
        { }
    }
}
