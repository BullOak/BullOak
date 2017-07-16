namespace BullOak.EventStream
{
    using System;
    using System.Collections.Generic;

    public struct EventStoreData
    {
        public int ConcurrencyId { get; }

        public IEnumerable<IParcelVisionEventEnvelope> EventEnvelopes { get; }

        public EventStoreData(IEnumerable<IParcelVisionEventEnvelope> events, int concurrencyId)
        {
            EventEnvelopes = events ?? throw new ArgumentException(nameof(events));
            ConcurrencyId = concurrencyId;
        }
    }
}
