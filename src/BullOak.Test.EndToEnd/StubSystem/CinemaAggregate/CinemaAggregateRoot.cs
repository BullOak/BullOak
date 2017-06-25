namespace BullOak.Test.EndToEnd.StubSystem.CinemaAggregate
{
    using BullOak.Application;
    using BullOak.Test.EndToEnd.StubSystem.Messages;
    using BullOak.Test.EndToEnd.StubSystem.ViewingAggregate;
    using System;

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
