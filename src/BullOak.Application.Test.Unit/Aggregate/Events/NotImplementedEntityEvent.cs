namespace BullOak.Application.Test.Unit.Aggregate.Events
{
    using System;
    using BullOak.Messages;

    public class NotImplementedEntityEvent : ParcelVisionEvent
    {
        public AggregateRootTestId AggregateSutId { get; set; }

        public Guid EntityId { get; set; }

        public NotImplementedEntityEvent(Guid correlationId, Guid entityId, AggregateRootTestId aggregateSutId)
            : base(correlationId)
        {
            AggregateSutId = aggregateSutId;
            EntityId = entityId;
        }
    }
}
