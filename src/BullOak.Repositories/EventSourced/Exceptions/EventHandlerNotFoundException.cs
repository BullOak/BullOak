namespace BullOak.Repositories.EventSourced.Exceptions
{
    using System;

    public class EventHandlerNotFoundException : Exception
    {
        public EventHandlerNotFoundException(Type eventType)
            :this(eventType.AssemblyQualifiedName)
        { }

        public EventHandlerNotFoundException(string eventName)
            : base($"Could not find event handler of type IReconstituteStateFromEvents for event type {eventName}")
        { }

        public static EventHandlerNotFoundException FromEnvelope(IHoldEventWithMetadata envelope)
        {
            return new EventHandlerNotFoundException(envelope.EventType);
        }
    }
}
