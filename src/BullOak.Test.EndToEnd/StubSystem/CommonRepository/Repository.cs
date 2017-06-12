namespace BullOak.Test.EndToEnd.StubSystem.CommonRepository
{
    using BullOak.Application;
    using BullOak.Common;
    using System;
    using BullOak.EventStream;

    internal class Repository<TAggregateRoot, TId> : AggregateRepositoryBase<TAggregateRoot, TId>
        where TId : IId, IEquatable<TId>
        where TAggregateRoot : AggregateRoot<TId>, new()
    {
        public Repository(IEventStore eventStore)
            :base(eventStore)
        { }
    }
}
