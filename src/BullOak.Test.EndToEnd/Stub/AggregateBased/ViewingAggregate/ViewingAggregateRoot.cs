namespace BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate
{
    using System;
    using System.Collections.Generic;
    using BullOak.Application;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    public class ViewingAggregateRoot : AggregateRoot<ViewingId>,
        IPublish<ViewingCreatedEvent>,
        IHaveChildEntities<SeatsInViewing, SeatId>
    {
        private readonly Dictionary<SeatId, SeatsInViewing> seats = new Dictionary<SeatId, SeatsInViewing>();

        //for use by framework
        public ViewingAggregateRoot() { }

        public ViewingAggregateRoot(int numberOfSeats, DateTime timeOfViewing, string movieName, CinemaAggregateRootId cinemaId)
        {
            ApplyEvent(new ViewingCreatedEvent(new ViewingId(movieName, timeOfViewing, cinemaId), numberOfSeats));

            for (int i = 0; i < numberOfSeats; i++)
            {
                var seat = new SeatsInViewing(i);
                seat.SetParent(this);
            }
        }

        public void ReserveSeat(int seat)
        {
            seats[new SeatId((ushort) seat)].Reserve();
        }

        public void Apply(ViewingCreatedEvent @event)
        {
            Id = @event.Id;
        }

        public SeatsInViewing GetOrAdd(SeatId id, Func<SeatId, SeatsInViewing> factory)
        {
            if (!seats.TryGetValue(id, out var seat))
            {
                seat = factory(id);
                seats[id] = seat;
            }

            return seat;
        }
    }
}
