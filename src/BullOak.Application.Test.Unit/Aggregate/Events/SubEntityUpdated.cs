namespace BullOak.Application.Test.Unit.Aggregate.Events
{
    using System;
    using BullOak.Messages;

    public class SubEntityUpdated : ParcelVisionEvent
    {
        public SubEntityId SubEntityId { get; set; }

        public string Name { get; set; }

        public SubEntityUpdated(Guid correlationId, SubEntityId subEntityId, string name)
            : base(correlationId)
        {
            SubEntityId = subEntityId;
            Name = name;
        }
    }
}
