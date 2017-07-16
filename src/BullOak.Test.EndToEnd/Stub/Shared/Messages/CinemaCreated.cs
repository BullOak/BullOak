namespace BullOak.Test.EndToEnd.Stub.Shared.Messages
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public class CinemaCreated : BullOak.Messages.ParcelVisionEvent
    {
        public CinemaAggregateRootId Id { get; }
        public int Capacity { get; }
        public CinemaCreated(Guid correlationId, CinemaAggregateRootId id, int capacity) : base(correlationId)
        {
            Id = id;
            Capacity = capacity;
        }
    }
}
