namespace BullOak.Test.EndToEnd.Stub.AggregateBased.CinemaAggregate
{
    using System;
    using BullOak.Application;
    using BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    public class CinemaAggregateRoot : AggregateRoot<CinemaAggregateRootId>, IPublish<CinemaCreated>
    {
        public int NumberOfSeats { get; private set; }

        public CinemaAggregateRoot() { } //For use in reconstitution

        public CinemaAggregateRoot(Guid correlationId, int capacity, string name)
        {
            ApplyEvent(new CinemaCreated(correlationId, new CinemaAggregateRootId(name), capacity));
        }

        public ViewingAggregateRoot CreateViewing(DateTime timeOfViewing, string movieName)
            => new ViewingAggregateRoot(NumberOfSeats, timeOfViewing, movieName);

        void IPublish<CinemaCreated>.Apply(CinemaCreated @event)
        {
            Id = @event.Id;
            NumberOfSeats = @event.Capacity;
        }
    }
}
