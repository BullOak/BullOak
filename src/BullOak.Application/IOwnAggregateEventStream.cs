namespace BullOak.Application
{
    using System.Collections.Generic;
    using BullOak.EventStream;
    using Messages;

    internal interface IOwnAggregateEventStream
    {
        IParcelVisionEventEnvelope[] GetUncommitedEventsForAggregate();
        void ReconstituteAggregate(IEnumerable<IParcelVisionEventEnvelope> eventStream, int concurrencyId);
        void ClearUncommitedEvents();
    }
}