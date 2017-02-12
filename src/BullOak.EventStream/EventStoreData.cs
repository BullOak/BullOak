namespace BullOak.EventStream
{
    using BullOak.Messages;
    using System;
    using System.Collections.Generic;

    public struct EventStoreData
    {
        public int ConcurrencyId { get; private set; }

        public IEnumerable<IParcelVisionEventEnvelope> EventEnvelopes { get; private set; }

        public EventStoreData(IEnumerable<IParcelVisionEventEnvelope> events, int concurrencyId)
        {
            if (events == null) { throw new ArgumentException(nameof(events)); }

            ConcurrencyId = concurrencyId;
            EventEnvelopes = events;
        }
    }
}
