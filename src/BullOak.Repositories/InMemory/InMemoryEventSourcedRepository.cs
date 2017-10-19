namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Common;
    using BullOak.EventStream;
    using BullOak.Repositories.EventSourced;

    public class InMemoryEventSourcedRepository<TState, TId> : IManagePersistenceOf<TState, EventSourceSession<TState, int>, TId>
        where TId : IId
        where TState : new()
    {
        private static readonly Task done = Task.FromResult(0);
        private ConcurrentDictionary<TId, List<object>> eventStore = new ConcurrentDictionary<TId, List<object>>();
        private IEnumerable<IApplyEvents<TState>> appliers;

        public List<object> this[TId id] => eventStore[id];

        public InMemoryEventSourcedRepository(ICreateEventAppliers appliersFactory)
            => appliers = (appliersFactory ?? throw new ArgumentNullException(nameof(appliersFactory)))
            .GetInstance<TState>();

        public EventSourceSession<TState, int> Load(TId id, bool throwIfNotExists = false)
        {
            List<object> eventStream;

            if (!eventStore.TryGetValue(id, out eventStream))
            {
                eventStream = new List<object>();
                eventStore.GetOrAdd(id, eventStream);
            }

            var session = new InMemoryEventSourcedSession<TState>(appliers, (events, concurrency) => SaveEvents(id, events, concurrency));
            session.Initialize(eventStream.ToArray(), eventStream.Count);

            return session;
        }

        private Task SaveEvents(TId id, List<object> newEvents, int concurrencyId)
        {
            var list = eventStore[id];

            if(list.Count != concurrencyId) throw new ConcurrencyException(id.ToString(), typeof(TState));

            list.AddRange(newEvents);

            return done;
        }

        public Task Clear(TId id)
        {
            if(eventStore.ContainsKey(id)) eventStore[id].Clear();

            return done;
        }

        public Task<bool> Exists(TId id)
            => Task.FromResult(eventStore.ContainsKey(id));
    }
}
