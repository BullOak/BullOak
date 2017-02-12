namespace BullOak.Application.Test.Unit.Aggregate.Events
{
    using System;
    using BullOak.Messages;

    public class EntityAdded : ParcelVisionEvent
    {
        public EntityId EntityId { get; set; }

        public string Name { get; set; }

        public EntityAdded(Guid correlationId,  EntityId entityId, string name)
            : base(correlationId)
        {
            EntityId = entityId;
            Name = name;
        }
    }
}
