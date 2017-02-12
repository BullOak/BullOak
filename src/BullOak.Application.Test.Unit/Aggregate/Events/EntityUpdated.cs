namespace BullOak.Application.Test.Unit.Aggregate.Events
{
    using System;
    using BullOak.Messages;

    public class EntityUpdated : ParcelVisionEvent
    {
        public AggregateRootTestId AggregateSutId { get; set; }

        public EntityId EntityId { get; set; }

        public string Name { get; set; }

        public EntityUpdated(Guid correlationId, AggregateRootTestId aggregateSutId, EntityId entityId, string name)
            : base(correlationId)
        {
            AggregateSutId = aggregateSutId;
            EntityId = entityId;
            Name = name;
        }
    }
}
