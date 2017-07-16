namespace BullOak.Repositories.EventSourced
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.EventSourced.Exceptions;

    public abstract class BaseSession<TState> : IManageStoreRequestLifetime<TState>
    {
        private readonly Dictionary<Type, IReconstituteStateFromEvents<TState>> eventHandlers;
        private readonly List<IHoldEventWithMetadata> newEvents = new List<IHoldEventWithMetadata>();

        private Lazy<TState> state;
        public TState State => state.Value;

        protected BaseSession(Dictionary<Type, IReconstituteStateFromEvents<TState>> eventHandlers,
            Func<TState> stateFactory)
        {
            stateFactory = stateFactory ?? throw new ArgumentNullException(nameof(stateFactory));
            this.eventHandlers = eventHandlers ?? throw new ArgumentNullException(nameof(eventHandlers));

            state = new Lazy<TState>(() => LoadEvents()
                .Aggregate(stateFactory(), ProcessEvent));
        }

        protected abstract IHoldEventWithMetadata[] LoadEvents();

        private TState ProcessEvent(TState state, object @event)
        {
            var eventWithMeta = @event as IHoldEventWithMetadata;

            if (eventWithMeta == null)
                throw new EventEnvelopeNotRecognisedException(@event.GetType(),
                    typeof(IHoldEventWithMetadata));

            IReconstituteStateFromEvents<TState> handler;
            if (eventHandlers.TryGetValue(eventWithMeta.EventType, out handler))
            {
                return handler.Apply(state, eventWithMeta);
            }

            throw EventHandlerNotFoundException.FromEnvelope(eventWithMeta);
        }

        public Task SaveChanges()
        {
            return Save(newEvents);
        }

        protected abstract Task Save(List<IHoldEventWithMetadata> eventsToStore);

        public void AddToStream(IHoldEventWithMetadata @event) => newEvents.Add(@event);
        public void AddToStream(IEnumerable<IHoldEventWithMetadata> events) => newEvents.AddRange(events);

        protected virtual void Dispose(bool disposing)
        {
            //Nothing to do here...
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
