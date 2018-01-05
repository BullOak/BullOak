namespace BullOak.Repositories.Session
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventPublisher;

    public abstract class BaseEventSourcedSyncSession<TState, TConcurrencyId> : BaseRepoSession<TState>, IManageAndSaveSynchronousSessionWithExplicitSnapshot<TState>
    {
        private static readonly Type typeOfState = typeof(TState);
        private static readonly Task done = Task.FromResult(0);

        private TConcurrencyId concurrencyId;
        public TConcurrencyId ConcurrencyId => concurrencyId;

        protected readonly IApplyEventsToStates eventApplier;
        protected readonly IPublishEvents eventPublisher;

        public sealed override bool IsOptimisticConcurrencySupported => true;

        public BaseEventSourcedSyncSession(IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(configuration, disposableHandle)
        {
            eventApplier = configuration.EventApplier;
            eventPublisher = configuration.EventPublisher;
        }

        public void LoadFromEvents(object[] storedEvents, TConcurrencyId concurrencyId)
        {
            var initialState = configuration.StateFactory.GetState(typeOfState);
            int storedEventsCount = storedEvents.Length;
            Type eventType = null;
            for (int i =0;i<storedEventsCount;i++)
            {
                eventType = storedEvents[i].GetType();
                initialState = eventApplier.Apply(typeOfState, initialState, eventType, storedEvents[i]);
            }

            Initialize((TState) initialState);
            this.concurrencyId = concurrencyId;
        }

        public void SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
            => SaveChanges(false, targetGuarantee);

        public void SaveChangesWithSnapshot(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
            => SaveChanges(false, targetGuarantee);

        private void SaveChanges(bool shouldSnapshot = false,
            DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            TState currentState;
            if (shouldSnapshot)
                currentState = GetCurrentState();
            else
                currentState = default(TState);

            if (sendEventsBeforeSaving) PublishEvents(configuration, newEvents);
            SaveChanges(newEvents, shouldSnapshot, currentState);
            if (!sendEventsBeforeSaving) PublishEvents(configuration, newEvents);
        }

        protected void PublishEvents(IHoldAllConfiguration configuration, object[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                eventPublisher.PublishSync(events[i]);
            }
        }

        protected abstract void SaveChanges(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot);
    }
}