namespace BullOak.Application.Test.Unit.Aggregate.Events
{
    using System;
    using BullOak.Messages;

    public class AggregateCreated : ParcelVisionEvent
    {
        public AggregateRootTestId AggregateSutId { get; set; }

        public string Name { get; set; }

        public AggregateCreated(Guid correlationId, AggregateRootTestId aggregateSutId, string name)
            : base(correlationId)
        {
            AggregateSutId = aggregateSutId;
            Name = name;
        }
    }
}
