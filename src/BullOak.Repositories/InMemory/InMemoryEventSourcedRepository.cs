namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.Common;
    using BullOak.EventStream;
    using BullOak.Repositories.EventSourced;

    public class InMemoryEventSourcedRepository<TState, TId> : IManagePersistenceOf<TState, EventSourceSession<TState, int>, TId>
        where TId : IId
        where TState : new()
    {
        private Dictionary<TId, List<IHoldEventWithMetadata>> eventStore = new Dictionary<TId, List<IHoldEventWithMetadata>>();
        private ICreateEventAppliers appliersFactory;

        public List<IHoldEventWithMetadata> this[TId id]
        {
            get
            {
                lock (eventStore)
                {
                    return eventStore[id];
                }
            }
        }

        public InMemoryEventSourcedRepository(ICreateEventAppliers appliersFactory)
            => this.appliersFactory = appliersFactory ?? throw new ArgumentNullException(nameof(appliersFactory));

        public EventSourceSession<TState, int> Load(TId id, bool throwIfNotExists = false)
        {
            lock (eventStore)
            {
                List<IHoldEventWithMetadata> eventStream;

                if (!eventStore.TryGetValue(id, out eventStream))
                {
                    eventStream = new List<IHoldEventWithMetadata>();
                    eventStore.Add(id, eventStream);
                }

                var session = new InMemoryEventSourcedSession<TState>(appliersFactory, (events, concurrency) => SaveEvents(id, events, concurrency));
                session.Initialize(eventStream.ToArray(), eventStream.Count);

                return session;
            }
        }

        private Task SaveEvents(TId id, List<IHoldEventWithMetadata> newEvents, int concurrencyId)
        {
            lock (eventStore)
            {
                var list = eventStore[id];

                if(list.Count != concurrencyId) throw new ConcurrencyException(id.ToString(), typeof(TState));

                list.AddRange(newEvents);
            }

            return Task.FromResult(0);
        }

        public Task Clear(TId id)
        {
            lock (eventStore)
            {
                if(eventStore.ContainsKey(id)) eventStore[id].Clear();
            }

            return Task.FromResult(0);
        }

        public Task<bool> Exists(TId id)
        {
            lock (eventStore)
            {
                return Task.FromResult(eventStore.ContainsKey(id));
            }
        }
    }
}
