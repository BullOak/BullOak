namespace BullOak.Repositories.EventSourced
{
    using System;
    using System.Collections.Generic;

    public class EventEnvelope<TEventType> : IHoldEventWithMetadata<TEventType>
    {
        private TEventType @event;

        public Type EventType { get; }
        public Dictionary<string, string> Metadata { get; }
        public TEventType Event => @event;
        object IHoldEventWithMetadata.Event => @event;

        public EventEnvelope(TEventType @event)
        {
            this.@event = @event;
            EventType = @event.GetType();
            Metadata = new Dictionary<string, string>();
        }
    }
}
