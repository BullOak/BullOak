namespace BullOak.Test.EndToEnd.Stub.Shared.Messages
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public class CinemaCreated : BullOakEvent
    {
        public CinemaAggregateRootId CinemaId { get; }
        public int Capacity { get; }
        public CinemaCreated(Guid correlationId, CinemaAggregateRootId cinemaId, int capacity) : base(correlationId)
        {
            CinemaId = cinemaId;
            Capacity = capacity;
        }
    }
}
