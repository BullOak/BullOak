namespace BullOak.Application.Test.Unit.Aggregate.Events
{
    using System;
    using BullOak.Messages;

    public class SubEntityCreated : ParcelVisionEvent
    {
        public SubEntityId SubEntityId { get; set; }

        public string Name { get; set; }

        public SubEntityCreated(Guid correlationId, SubEntityId subEntityId, string name)
            : base(correlationId)
        {
            SubEntityId = subEntityId;
            Name = name;
        }
    }
}
