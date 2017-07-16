namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.Repositories.EventSourced;

    public interface IManageStoreRequestLifetime<TState> : IDisposable
    {
        TState State { get; }
        Task SaveChanges();

        void AddToStream(IHoldEventWithMetadata @event);
        void AddToStream(IEnumerable<IHoldEventWithMetadata> events);
    }
}