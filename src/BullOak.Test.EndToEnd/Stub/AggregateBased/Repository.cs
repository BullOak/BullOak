namespace BullOak.Test.EndToEnd.Stub.AggregateBased
{
    using System;
    using BullOak.Application;
    using BullOak.Common;
    using BullOak.EventStream;

    public class Repository<TAggregateRoot, TId> : AggregateRepositoryBase<TAggregateRoot, TId>
        where TId : IId, IEquatable<TId>
        where TAggregateRoot : AggregateRoot<TId>, new()
    {
        public Repository(IEventStore eventStore)
            :base(eventStore)
        { }
    }
}
