namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventPublisher;
    using BullOak.Repositories.Upconverting;

    public abstract class BaseRepoSession<TState> : IManageSessionOf<TState>
    {
        private readonly IDisposable disposableHandle;

        protected readonly IHoldAllConfiguration configuration;
        protected ICollection<object> NewEventsCollection { get; private set; }

        public abstract bool IsOptimisticConcurrencySupported { get; }
        private TState currentState;
        public TState GetCurrentState() => currentState;

        protected readonly IPublishEvents eventPublisher;

        private static readonly Type stateType = typeof(TState);
        private static object eventApplierLock = new object();
        protected static IApplyEventsToStates EventApplier;

        protected BaseRepoSession(IHoldAllConfiguration configuration, IDisposable disposableHandle)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.disposableHandle = disposableHandle;

            NewEventsCollection = configuration.CollectionTypeSelector(stateType)();

            currentState = default(TState);

            this.eventPublisher = configuration.EventPublisher;

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

            currentState =
                (TState) EventApplier.Apply(stateType, currentState, events.Select(x => new ItemWithType(x)));
        }

        public void AddEvents(object[] events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            for (int i = 0; i < events.Length; i++)
                NewEventsCollection.Add(events[i]);

            currentState =
                (TState) EventApplier.Apply(stateType, currentState, events.Select(x => new ItemWithType(x)));
        }

        public void AddEvent(object @event)
        {
            if (@event is ICollection eventCollection)
            {
                object[] eventArray = new object[eventCollection.Count];
                eventCollection.CopyTo(eventArray, 0);
                AddEvents(eventArray);
            }
            else if (@event is IEnumerable<object> events)
                AddEvents(events);
            else
            {
                NewEventsCollection.Add(@event);
                currentState = (TState) EventApplier.ApplyEvent(stateType, currentState, new ItemWithType(@event));
            }
        }

        protected void Initialize(TState storedState)
            => currentState = storedState;

        protected async Task PublishEvents(IHoldAllConfiguration configuration, object[] events, CancellationToken? cancellationToken = null)
        {
            for (var i = 0; i < events.Length; i++)
            {
                //We await each one to guarantee ordering of publishing, even though it would have been more performant
                // to publish and await all of them with a WhenAll
                await eventPublisher.Publish(events[i], cancellationToken);
            }
        }

        protected void PublishEventsSync(IHoldAllConfiguration configuration, object[] events)
        {
            for (var i = 0; i < events.Length; i++)
            {
                eventPublisher.PublishSync(events[i]);
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