namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class EventEnvelope<TEvent> : IHoldEventWithMetadata<TEvent>
    {
        public Type EventType => Event.GetType();
        public Dictionary<string, string> Metadata { get; } = new Dictionary<string, string>(0);
        public TEvent Event { get; private set; }
        object IHoldEventWithMetadata.Event => Event;

        public EventEnvelope(TEvent @event) => Event = @event;
    }

    internal static class EnvelopeHelper
    {
        public static EventEnvelope<TEvent> CreateEnvelopeDynamic<TEvent>(TEvent @event)
            => new EventEnvelope<TEvent>(@event);
        public static IHoldEventWithMetadata CreateEnvelope(dynamic @event)
            => CreateEnvelopeDynamic(@event);
    }

    public abstract class BaseSession<TState> : IManageSessionOf<TState>
        where TState : new()
    {
        internal readonly ICreateEventAppliers eventApplyFactory;
        internal readonly List<IHoldEventWithMetadata> eventsToStore = new List<IHoldEventWithMetadata>(4);
        public IEnumerable<IHoldEventWithMetadata> NewEvents => eventsToStore.AsReadOnly();

        public TState State => GetCurrent();

        internal BaseSession(ICreateEventAppliers factory)
            => this.eventApplyFactory = factory ?? throw new ArgumentNullException(nameof(factory));

        protected abstract TState GetCurrent();

        protected TState ApplyEvent(TState state, IHoldEventWithMetadata eventEnvelope)
        {
            //TODO: This method is looping and is already in a loop AND a hot path
            var appliers = eventApplyFactory.GetInstance<TState>();

            foreach (var applier in appliers.Where(x=> x.CanApplyEvent(eventEnvelope.Event)))
            {
                state = applier.Apply(state, eventEnvelope);
            }

            return state;
        }

        public abstract Task SaveChanges();

        public void AddToStream(IEnumerable<object> events)
            => eventsToStore.AddRange(events.Select(EnvelopeHelper.CreateEnvelope));
        public void AddToStream(params IHoldEventWithMetadata[] events)
            => eventsToStore.AddRange(events);
        public void AddToStream(params object[] events)
            => eventsToStore.AddRange(events.Select(EnvelopeHelper.CreateEnvelope));

        public void Dispose() => eventsToStore.Clear();
    }
}
