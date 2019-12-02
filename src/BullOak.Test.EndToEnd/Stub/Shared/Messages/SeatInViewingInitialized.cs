namespace BullOak.Test.EndToEnd.Stub.Shared.Messages
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public class SeatInViewingInitialized : BullOakEvent
    {
        public SeatId SeatId { get; private set; }

        public SeatInViewingInitialized(SeatId idOfSeatToReserve)
            : base(Guid.NewGuid())
        {
            SeatId = idOfSeatToReserve;
        }
    }
}
