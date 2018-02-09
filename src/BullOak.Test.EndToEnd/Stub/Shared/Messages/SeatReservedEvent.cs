namespace BullOak.Test.EndToEnd.Stub.Shared.Messages
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public class SeatReservedEvent : BullOakEvent
    {
        public ViewingId ViewingId { get; }
        public SeatId IdOfSeatToReserve { get; }

        public SeatReservedEvent(ViewingId viewingId, SeatId idOfSeatToReserve)
            :base(Guid.NewGuid())
        {
            ViewingId = viewingId;
            IdOfSeatToReserve = idOfSeatToReserve;
        }
    }
}