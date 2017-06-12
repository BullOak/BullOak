namespace BullOak.Test.EndToEnd.StubSystem.Messages
{
    using BullOak.Test.EndToEnd.StubSystem.CinemaAggregate;
    using System;

    internal class CinemaCreated : BullOak.Messages.ParcelVisionEvent
    {
        public CinemaAggregateRootId Id { get; private set; }
        public int Capacity { get; private set; }
        public CinemaCreated(Guid correlationId, CinemaAggregateRootId id, int capacity) : base(correlationId)
        {
            Id = id;
            Capacity = capacity;
        }
    }
}
