namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Session.CustomLinkedList;
    using BullOak.Repositories.Session.StateUpdaters;

    public abstract class BaseRepoSession<TState> : IManageSessionOf<TState>
    {
        private static readonly Type typeOfState = typeof(TState);

        private readonly IDisposable disposableHandle;

        protected readonly IHoldAllConfiguration configuration;
        private IApplyEventsToCurrentState<TState> eventApplier;
        protected IApplyEventsToCurrentState<TState> EventApplier
        {
            get => eventApplier;
            private set => eventApplier = eventApplier ?? value;
        }
        protected ICollection<object> NewEventsCollection { get; private set; }

        public abstract bool IsOptimisticConcurrencySupported { get; }
        public TState GetCurrentState() => EventApplier.GetCurrentState();

        private static readonly object collectionFactoryLock = new object();
        private static Func<ICollection<object>> newEventCollectionFactory;

        internal BaseRepoSession(IHoldAllConfiguration configuration, IDisposable disposableHandle)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.disposableHandle = disposableHandle;

            if (newEventCollectionFactory == null)
            {
                lock (collectionFactoryLock)
                {
                    if (newEventCollectionFactory == null)
                    {
                        newEventCollectionFactory = this.configuration.CollectionTypeSelector(typeof(TState));
                    }
                }
            }
        }

        public void AddEvents(IEnumerable<object> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
                NewEventsCollection.Add(@event);
        }

        public void AddEvents(object[] events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            for (int i = 0; i < events.Length; i++)
                NewEventsCollection.Add(events[i]);
        }

        public void AddEvent(object @event)
        {
            if (@event is IEnumerable<object> events) AddEvents(events);
            else if (@event is ICollection eventCollection)
            {
                object[] eventArray = new object[eventCollection.Count];
                eventCollection.CopyTo(eventArray, 0);
            }
            else NewEventsCollection.Add(@event);
        } 

        protected void Initialize(TState storedState)
        {
            NewEventsCollection = newEventCollectionFactory();
            EventApplier = GetApplierFor(configuration, storedState, NewEventsCollection);
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
        {
            if(disposing) disposableHandle?.Dispose();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}