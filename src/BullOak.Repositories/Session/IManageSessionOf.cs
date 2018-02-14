namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages a session. A session represents the operations that relate
    /// to loading from db, mutating the state (in BullOak this happens exclusively through emitting
    /// events that may or may not be the data stored depending if you chose an ES repository) and
    /// then saving it. This does not hold a sync construct, but this optimistic concurrency may
    /// still be supported implicitly by the underlying store.
    /// </summary>
    /// <typeparam name="TState">The type of the state that this session manages</typeparam>
    public interface IManageSessionOf<out TState> : IDisposable
    {
        bool IsNewState { get; }
        TState GetCurrentState();

        void AddEvents(IEnumerable<object> events);
        void AddEvents(object[] events);
        void AddEvent(object @event);

        Task<int> SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}