namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    public class CinemaAggregateRoot
    {
        public int NumberOfSeats { get; private set; }

        public CinemaAggregateRoot() { } //For use in reconstitution

        public CinemaCreated Create(Guid correlationId, int capacity, string name)
        {
            return new CinemaCreated(correlationId, new CinemaAggregateRootId(name), capacity);
        }
    }
}
