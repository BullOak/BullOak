namespace BullOak.Repositories.Session
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventPublisher;

    public abstract class BaseEventSourcedSession<TState, TConcurrencyId> : BaseRepoSession<TState>, IManageAndSaveSessionWithExplicitSnapshot<TState>
    {
        private static readonly Type typeOfState = typeof(TState);

        private TConcurrencyId concurrencyId;
        public TConcurrencyId ConcurrencyId => concurrencyId;

        protected readonly IApplyEventsToStates eventApplier;
        protected readonly IPublishEvents eventPublisher;

        public sealed override bool IsOptimisticConcurrencySupported => true;

        public BaseEventSourcedSession(IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(configuration, disposableHandle)
        {
            eventApplier = configuration.EventApplier;
            eventPublisher = configuration.EventPublisher;
        }

        public void LoadFromEvents(object[] storedEvents, TConcurrencyId concurrencyId)
        {
            var initialState = configuration.StateFactory.GetState(typeOfState);

            initialState = eventApplier.Apply(typeOfState, initialState, storedEvents);

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

        protected async Task PublishEvents(IHoldAllConfiguration configuration, object[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                //We await each one to guarantee ordering of publishing, even though it would have been more performant
                // to publish and await all of them with a WhenAll
                await eventPublisher.Publish(events[i]);
            }
        }

        protected abstract Task SaveChanges(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot);
    }
}