namespace BullOak.Test.EndToEnd.Stub.Shared.Messages
{
    using System;
    using BullOak.Messages;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public class SeatInViewingInitialized : ParcelVisionEvent, IParcelVisionEvent
    {
        public SeatId SeatId { get; private set; }

        public SeatInViewingInitialized(SeatId idOfSeatToReserve)
            : base(Guid.NewGuid())
        {
            SeatId = idOfSeatToReserve;
        }
    }
}