namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.EventSourced;
    using global::NEventStore;

    internal class NEventStoreSession<TState> : BaseSession<TState>
    {
        private readonly IEventStream eventStream;

        public NEventStoreSession(Dictionary<Type, IReconstituteStateFromEvents<TState>> eventHandlers,
            Func<TState> stateFactory,
            IEventStream eventStream)
            : base(eventHandlers, stateFactory)
        {
            this.eventStream = eventStream ?? throw new ArgumentNullException(nameof(eventStream));
        }

        protected override IHoldEventWithMetadata[] LoadEvents() =>
            eventStream.CommittedEvents
                .Select(x => x.Body as IHoldEventWithMetadata)
                .Where(x => x != null)
                .ToArray();

        protected override Task Save(List<IHoldEventWithMetadata> eventsToStore)
        {
            foreach (var @event in eventsToStore)
            {
                eventStream.Add(new EventMessage()
                {
                    Body = @event
                });
            }

            eventStream.CommitChanges(Guid.NewGuid());
            return Task.FromResult(0);
        }
    }
}
