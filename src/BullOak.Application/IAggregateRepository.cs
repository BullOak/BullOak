namespace BullOak.Application
{
    using System.Threading.Tasks;
    using System;
    using BullOak.Common;

    public interface IAggregateRepository<TAggregateRoot, TId>
        where TId : IId, IEquatable<TId>
        where TAggregateRoot : AggregateRoot<TId>
    {
        Task<bool> Exists(string id);

        Task<TAggregateRoot> Load(TId id, bool throwIfNotFound = true);

        Task Save(TAggregateRoot aggregateRoot);
    }

}
