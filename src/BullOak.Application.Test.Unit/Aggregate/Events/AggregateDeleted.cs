namespace BullOak.Application.Test.Unit.Aggregate.Events
{
    using System;
    using BullOak.Messages;

    public class AggregateDeleted : ParcelVisionEvent
    {
        public AggregateRootTestId AggregateSutId { get; set; }

        public string Name { get; set; }

        public AggregateDeleted(Guid correlationId, AggregateRootTestId aggregateSutId)
            : base(correlationId)
        {
            AggregateSutId = aggregateSutId;
        }
    }
}
