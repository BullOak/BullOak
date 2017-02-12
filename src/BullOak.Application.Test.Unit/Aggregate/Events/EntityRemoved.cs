namespace BullOak.Application.Test.Unit.Aggregate.Events
{
    using System;
    using BullOak.Messages;

    public class EntityRemoved : ParcelVisionEvent
    {
        public AggregateRootTestId AggregateSutId { get; set; }

        public EntityId EntityId { get; set; }

        public EntityRemoved(Guid correlationId, AggregateRootTestId aggregateSutId, EntityId entityId)
            : base(correlationId)
        {
            AggregateSutId = aggregateSutId;
            EntityId = entityId;
        }
    }
}
