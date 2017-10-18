namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    public class CinemaAggregateRoot
    {
        public CinemaCreated Create(Guid correlationId, int capacity, string name)
            => new CinemaCreated(correlationId, new CinemaAggregateRootId(name), capacity);


    }
}
