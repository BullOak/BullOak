namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate
{
    using System;
    using System.Collections.Generic;
    using BullOak.Messages;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    internal class ViewingAggregateRoot
    {
        private readonly SeatInViewingEntity seatInViewingEntity = new SeatInViewingEntity();

        public IEnumerable<IParcelVisionEvent> CreateViewing(CinemaAggregateRootId cinemaId, string movieName, DateTime timeOfViewing, int numberOfSeats)
        {
            yield return new ViewingCreatedEvent(new ViewingId(movieName, timeOfViewing, cinemaId), numberOfSeats);

            for (int i = 0; i < numberOfSeats; i++)
            {
                yield return seatInViewingEntity.Initialize(i);
            }
        }

        public SeatReservedEvent ReserveSeat(IViewingState state, int idOfSeatToReserve)
        {
            return seatInViewingEntity.Reserve(state, idOfSeatToReserve);
        }
    }
}
