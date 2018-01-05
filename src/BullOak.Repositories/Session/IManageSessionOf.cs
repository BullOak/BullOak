namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections.Generic;
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
        bool IsOptimisticConcurrencySupported { get; }
        TState GetCurrentState();

        void AddEvents(IEnumerable<object> events);
        void AddEvents(object[] events);
        void AddEvent(object events);
    }

    public interface IManageAndSaveSession<out TState> : IManageSessionOf<TState>
    {
        Task SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce);
    }

    public interface IManageAndSaveSessionWithExplicitSnapshot<out TState> : IManageAndSaveSession<TState>
    {
        Task SaveChangesWithSnapshot(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce);
    }

    public interface IManageAndSaveSynchronousSession<out TState> : IManageSessionOf<TState>
    {
        void SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce);
    }

    public interface IManageAndSaveSynchronousSessionWithExplicitSnapshot<out TState> : IManageAndSaveSynchronousSession <TState>
    {
        void SaveChangesWithSnapshot(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce);
    }
}