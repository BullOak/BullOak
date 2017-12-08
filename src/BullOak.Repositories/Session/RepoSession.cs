namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Session.CustomLinkedList;
    using BullOak.Repositories.Session.StateUpdaters;

    public abstract class BaseRepoSession<TState> : IManageSessionOf<TState>
    {
        private static readonly Type typeOfState = typeof(TState);

        protected readonly IHoldAllConfiguration configuration;
        private IApplyEventsToCurrentState<TState> eventApplier;
        protected IApplyEventsToCurrentState<TState> EventApplier
        {
            get => eventApplier;
            private set => eventApplier = eventApplier ?? value;
        }
        protected ICollection<object> NewEventsCollection { get; private set; }

        public abstract bool IsOptimisticConcurrencySupported { get; }

        internal BaseRepoSession(IHoldAllConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public TState GetCurrentState() => EventApplier.GetCurrentState();

        public void AddToStream(IEnumerable<object> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
                NewEventsCollection.Add(@event);
        }

        public void AddToStream(object[] events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            for (int i = 0; i < events.Length; i++)
                NewEventsCollection.Add(events[i]);
        }
        public void AddEvent(object @event) => NewEventsCollection.Add(@event);

        protected void Initialize(TState storedState)
        {
            NewEventsCollection = configuration.CollectionTypeSelector(typeOfState)();
            EventApplier = GetApplierFor(configuration, storedState, NewEventsCollection);
        }

        public abstract Task SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce);

        protected async Task PublishEvents(IHoldAllConfiguration configuration, object[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                //We await each one to guarantee ordering of publishing, even though it would have been more performant
                // to publish and await all of them with a WhenAll
                await configuration.EventPublisher(events[i]);
            }
        }

        private static IApplyEventsToCurrentState<TState> GetApplierFor<TState>(IHoldAllConfiguration configuration, TState initialState, ICollection<object> collection)
        {
            switch (collection)
            {
                case ILinkedList<object> linkedList:
                    return new StateUpdaterForLinkedList<TState>(configuration, initialState, linkedList);
                case IList<object> list:
                    return new StateUpdaterForIList<TState>(configuration, initialState, list);
                default:
                    return new StateUpdaterForICollection<TState>(configuration, initialState, collection);
            }
        }

        protected virtual void Dispose(bool disposing)
        { }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class RepoSessionWithConcurrency<TState, TConcurrencyId> : BaseRepoSession<TState>
    {
        private static readonly Type typeOfState = typeof(TState);
        private TConcurrencyId concurrencyId;
        public sealed override bool IsOptimisticConcurrencySupported => true;

        protected RepoSessionWithConcurrency(IHoldAllConfiguration configuration) 
            : base(configuration)
        { }

        public void LoadFromState(TState storedState, TConcurrencyId concurrencyId)
        {
            base.Initialize(storedState);
            this.concurrencyId = concurrencyId;
        }

        public void LoadFromEvents(IEnumerable<object> storedEvents, TConcurrencyId concurrencyId)
            => LoadFromState(storedEvents.Aggregate((TState)configuration.StateFactory.GetState(typeOfState),
                configuration.EventApplier.Apply), concurrencyId);

        public sealed override async Task SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            if (sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
            await SaveChangesProtected(newEvents, EventApplier.GetCurrentState(), concurrencyId, sendEventsBeforeSaving);
            if (!sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
        }

        protected abstract Task SaveChangesProtected(object[] newEvents, TState latestState, TConcurrencyId concurrencyId, bool eventsAlreadySent);
    }

    public abstract class RepoSessionWithoutConcurrency<TState> : BaseRepoSession<TState>
    {
        private static readonly Type typeOfState = typeof(TState);
        public sealed override bool IsOptimisticConcurrencySupported => false;

        protected RepoSessionWithoutConcurrency(IHoldAllConfiguration configuration) 
            : base(configuration)
        { }

        public void LoadFromState(TState storedState)
        {
            base.Initialize(storedState);
        }

        public void LoadFromEvents(IEnumerable<object> storedEvents)
            => LoadFromState(storedEvents.Aggregate((TState) configuration.StateFactory.GetState(typeOfState),

                configuration.EventApplier.Apply));
        public sealed override async Task SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            var newEvents = NewEventsCollection.ToArray();
            var sendEventsBeforeSaving = targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce;

            if (sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
            await SaveChangesProtected(newEvents, EventApplier.GetCurrentState(), sendEventsBeforeSaving);
            if (!sendEventsBeforeSaving) await PublishEvents(configuration, newEvents);
        }

        protected abstract Task SaveChangesProtected(object[] newEvents, TState latestState, bool eventsAlreadySent);
    }

    public class BasicEventSourcedRepoSession<TState> : RepoSessionWithConcurrency<TState, int>, IDisposable
    {
        private readonly IDisposable disposableHandle;
        private readonly Func<object[], int, Task> saveFunc;

        public BasicEventSourcedRepoSession(IHoldAllConfiguration configuration,
            IDisposable disposableHandle,
            Func<object[], int, Task> saveFunc)
            : base(configuration)
        {
            this.disposableHandle = disposableHandle;
            this.saveFunc = saveFunc ?? throw new ArgumentNullException(nameof(saveFunc));
        }

        public BasicEventSourcedRepoSession(IHoldAllConfiguration configuration,
            Func<object[], int, Task> saveFunc)
            : this(configuration, null, saveFunc)
        { }

        protected override Task SaveChangesProtected(object[] newEvents, TState latestState, int concurrencyId, bool eventsAlreadySent) 
            => saveFunc(newEvents, concurrencyId);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposableHandle?.Dispose();
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}