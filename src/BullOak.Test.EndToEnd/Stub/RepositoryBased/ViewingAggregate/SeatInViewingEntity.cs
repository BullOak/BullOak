namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    internal class SeatInViewingEntity
    {
        public SeatReservedEvent Reserve(IViewingState state, int idOfSeatToReserve)
        {
            if (state.Seats[idOfSeatToReserve].IsReserved)
                throw new Exception("Seat already reserved");

            return new SeatReservedEvent(state.ViewingId, new SeatId((ushort)idOfSeatToReserve));
        }

        public SeatInViewingInitialized Initialize(int idOfSeatToReserve)
        {
            return new SeatInViewingInitialized(new SeatId((ushort)idOfSeatToReserve));
        }
    }
}
