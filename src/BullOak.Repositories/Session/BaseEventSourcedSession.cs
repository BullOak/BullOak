namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class BaseEventSourcedSession<TState, TConcurrencyId> : BaseRepoSession<TState>, IManageAndSaveSessionWithExplicitSnapshot<TState>
    {
        private static readonly Type typeOfState = typeof(TState);
        private static readonly Task done = Task.FromResult(0);

        private TConcurrencyId concurrencyId;
        public TConcurrencyId ConcurrencyId => concurrencyId;


        public sealed override bool IsOptimisticConcurrencySupported => true;

        public BaseEventSourcedSession(IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(configuration, disposableHandle)
        { }

        public void LoadFromEvents(IEnumerable<object> storedEvents, TConcurrencyId concurrencyId)
        {
            var initialState = storedEvents.Aggregate((TState) configuration.StateFactory.GetState(typeOfState),
                (s, e) => (TState)configuration.EventApplier.Apply(typeOfState, s, e.GetType(), e));
            Initialize(initialState);
            this.concurrencyId = concurrencyId;
        }

        public async Task SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            if (sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
            await SaveChanges(newEvents, false, default(TState));
            if (!sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
        }

        public async Task SaveChangesWithSnapshot(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            if (sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
            await SaveChanges(newEvents, true, EventApplier.GetCurrentState());
            if (!sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
        }

        protected async Task PublishEvents(IHoldAllConfiguration configuration, object[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                //We await each one to guarantee ordering of publishing, even though it would have been more performant
                // to publish and await all of them with a WhenAll
                await configuration.EventPublisher.Publish(events[i]);
            }
        }

        protected abstract Task SaveChanges(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot);
    }
}