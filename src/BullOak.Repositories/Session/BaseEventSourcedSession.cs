namespace BullOak.Repositories.Session
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventPublisher;

    public abstract class BaseEventSourcedSession<TState, TConcurrencyId> : BaseRepoSession<TState>, IManageAndSaveSessionWithSnapshot<TState>
    {
        private static readonly Type typeOfState = typeof(TState);

        private TConcurrencyId concurrencyId;
        public TConcurrencyId ConcurrencyId => concurrencyId;

        public sealed override bool IsOptimisticConcurrencySupported => true;

        public BaseEventSourcedSession(IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(configuration, disposableHandle)
        { }

        public void LoadFromEvents(object[] storedEvents, TConcurrencyId concurrencyId)
        {
            var initialState = configuration.StateFactory.GetState(typeOfState);

            initialState = EventApplier.Apply(typeOfState, initialState, storedEvents);

            Initialize((TState) initialState);
            this.concurrencyId = concurrencyId;
        }

        public Task SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
            => SaveChanges(false, targetGuarantee);

        public Task SaveChangesWithSnapshot(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
            => SaveChanges(true, targetGuarantee);

        private async Task SaveChanges(bool shouldSnapshot = false,
            DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            TState currentState;
            if (shouldSnapshot)
                currentState = GetCurrentState();
            else
                currentState = default(TState);

            if (sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
            await SaveChanges(newEvents, shouldSnapshot, currentState);
            if (!sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
        }
        protected abstract Task SaveChanges(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot);
    }
}