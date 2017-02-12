namespace BullOak.Application.Test.Unit.Aggregate.Events
{
    using System;
    using BullOak.Messages;

    public class NotImplementedAggregateEvent : ParcelVisionEvent
    {
        public AggregateRootTestId AggregateSutId { get; set; }

        public NotImplementedAggregateEvent(Guid correlationId, AggregateRootTestId aggregateSutId)
            : base(correlationId)
        {
            AggregateSutId = aggregateSutId;
        }
    }
}
