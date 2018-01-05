namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.StateEmit;

    public abstract class BaseEventSourcedSyncSession<TState, TConcurrencyId> : BaseRepoSession<TState>, IManageAndSaveSynchronousSessionWithExplicitSnapshot<TState>
    {
        private static readonly Type typeOfState = typeof(TState);
        private static readonly Task done = Task.FromResult(0);

        private TConcurrencyId concurrencyId;
        public TConcurrencyId ConcurrencyId => concurrencyId;

        protected readonly IApplyEventsToStates eventApplier;

        public sealed override bool IsOptimisticConcurrencySupported => true;

        public BaseEventSourcedSyncSession(IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(configuration, disposableHandle)
        {
            eventApplier = configuration.EventApplier;
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

        public void LoadFromEvents(IEnumerable<object> storedEvents, TConcurrencyId concurrencyId)
        {
            var initialState = configuration.StateFactory.GetState(typeOfState);

            Type eventType = null;
            foreach (var @event in storedEvents)
            {
                eventType = @event.GetType();
                initialState = eventApplier.Apply(typeOfState, initialState, eventType, @event);
            }

            Initialize((TState)initialState);
            this.concurrencyId = concurrencyId;
        }

        public void SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            if (sendEventsBeforeSaving) PublishEvents(configuration, newEvents);
            SaveChanges(newEvents, false, default(TState));
            if (!sendEventsBeforeSaving) PublishEvents(configuration, newEvents);
        }

        public void SaveChangesWithSnapshot(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            if (sendEventsBeforeSaving) PublishEvents(configuration, newEvents);
            SaveChanges(newEvents, true, EventApplier.GetCurrentState());
            if (!sendEventsBeforeSaving) PublishEvents(configuration, newEvents);
        }

        protected void PublishEvents(IHoldAllConfiguration configuration, object[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                //We await each one to guarantee ordering of publishing, even though it would have been more performant
                // to publish and await all of them with a WhenAll
                configuration.EventPublisher.PublishSync(events[i]);
            }
        }

        protected abstract void SaveChanges(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot);
    }
}