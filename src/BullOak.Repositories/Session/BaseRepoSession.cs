namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using BullOak.Repositories.Appliers;

    public abstract class BaseRepoSession<TState> : IManageSessionOf<TState>
    {
        private readonly IDisposable disposableHandle;

        protected readonly IHoldAllConfiguration configuration;
        protected ICollection<object> NewEventsCollection { get; private set; }

        public abstract bool IsOptimisticConcurrencySupported { get; }
        private TState currentState;
        public TState GetCurrentState() => currentState;

        private static readonly Type stateType = typeof(TState);
        private static object eventApplierLock = new object();
        protected static IApplyEventsToStates EventApplier;

        internal BaseRepoSession(IHoldAllConfiguration configuration, IDisposable disposableHandle)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.disposableHandle = disposableHandle;

            NewEventsCollection = configuration.CollectionTypeSelector(stateType)();

            currentState = default(TState);

            if (EventApplier == null)
            {
                lock (eventApplierLock)
                {
                    if (EventApplier == null)
                    {
                        EventApplier = this.configuration.EventApplier;
                    }
                }
            }
        }

        public void AddEvents(IEnumerable<object> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
                NewEventsCollection.Add(@event);

            currentState = (TState) EventApplier.Apply(stateType, currentState, events);
        }

        public void AddEvents(object[] events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            for (int i = 0; i < events.Length; i++)
                NewEventsCollection.Add(events[i]);

            currentState = (TState)EventApplier.Apply(stateType, currentState, events);
        }

        public void AddEvent(object @event)
        {
            if (@event is IEnumerable<object> events) AddEvents(events);
            else if (@event is ICollection eventCollection)
            {
                object[] eventArray = new object[eventCollection.Count];
                eventCollection.CopyTo(eventArray, 0);
                AddEvents(eventArray);
            }
            else
            {
                NewEventsCollection.Add(@event);
                currentState = (TState) EventApplier.ApplyEvent(stateType, currentState, @event);
            }
        } 

        protected void Initialize(TState storedState)
        {
            currentState = storedState;
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