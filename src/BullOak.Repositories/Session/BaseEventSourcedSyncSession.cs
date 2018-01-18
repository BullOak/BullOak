namespace BullOak.Repositories.Session
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Upconverting;

    public abstract class BaseEventSourcedSession<TState, TConcurrencyId> : BaseRepoSession<TState>, IManageAndSaveSessionWithSnapshot<TState>
    {
        private static readonly Type typeOfState = typeof(TState);
        private static readonly Task done = Task.FromResult(0);

        private TConcurrencyId concurrencyId;
        public TConcurrencyId ConcurrencyId => concurrencyId;

        protected readonly IApplyEventsToStates eventApplier;

        public sealed override bool IsOptimisticConcurrencySupported => true;

        public BaseEventSourcedSession(IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(configuration, disposableHandle)
            => eventApplier = configuration.EventApplier;

        public void LoadFromEvents(object[] storedEvents, TConcurrencyId concurrencyId)
        {
            var eventsWithTypes = storedEvents
                .Select(x => new ItemWithType(x))
                .ToArray();

            var upconvertedEvents = configuration
                .EventUpconverter
                .Upconvert(eventsWithTypes);

            var initialState = configuration.StateFactory.GetState(typeOfState);

            initialState = eventApplier.Apply(typeOfState, initialState, upconvertedEvents);

            Initialize((TState) initialState);
            this.concurrencyId = concurrencyId;
        }

        public int SaveChangesSync(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
            => SaveChangesSync(false, targetGuarantee);

        public int SaveChangesWithSnapshotSync(DeliveryTargetGuarntee targetGuarantee =
            DeliveryTargetGuarntee.AtLeastOnce)
            => SaveChangesSync(true, targetGuarantee);

        public Task<int> SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce,
            CancellationToken? cancellationToken = null)
            => SaveChanges(false, targetGuarantee, cancellationToken);

        public Task<int> SaveChangesWithSnapshot(DeliveryTargetGuarntee targetGuarantee =
            DeliveryTargetGuarntee.AtLeastOnce, CancellationToken? cancellationToken = null)
            => SaveChanges(true, targetGuarantee, cancellationToken);

        private async Task<int> SaveChanges(bool shouldSnapshot = false,
            DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce,
            CancellationToken? cancellationToken = null)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            TState currentState = shouldSnapshot ? GetCurrentState() : default(TState);

            if (sendEventsBeforeSaving) await PublishEvents(configuration, newEvents, cancellationToken);
            var retVal = await SaveChanges(newEvents, shouldSnapshot, currentState, cancellationToken);
            if (!sendEventsBeforeSaving) await PublishEvents(configuration, newEvents, cancellationToken);

            return retVal;
        }

        private int SaveChangesSync(bool shouldSnapshot = false,
            DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            TState currentState = shouldSnapshot ? GetCurrentState() : default(TState);

            if (sendEventsBeforeSaving) PublishEventsSync(configuration, newEvents);
            var retVal = SaveChangesSync(newEvents, shouldSnapshot, currentState);
            if (!sendEventsBeforeSaving) PublishEventsSync(configuration, newEvents);

            return retVal;
        }

        protected abstract Task<int> SaveChanges(object[] eventsToAdd,
            bool shouldSaveSnapshot,
            TState snapshot,
            CancellationToken? cancellationToken);

        protected abstract int SaveChangesSync(object[] eventsToAdd,
            bool shouldSaveSnapshot,
            TState snapshot);
    }
}