namespace BullOak.Messages.Converters.AutoMapper.Exceptions
{
    using System;

    public class AutomapperRegistrationRequiredException : Exception
    {
        public AutomapperRegistrationRequiredException(Type sourceEventType, Type destinationEventType, Type upconverterType)
            : base($"{upconverterType.Name} does not declare a method to create a Automapper mapping from {sourceEventType.Name} to {destinationEventType.Name}")
        { }

        public AutomapperRegistrationRequiredException(string message)
            : base(message)
        { }
    }
}