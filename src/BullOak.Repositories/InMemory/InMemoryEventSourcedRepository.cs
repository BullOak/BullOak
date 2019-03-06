namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Repository;
    using BullOak.Repositories.Session;

    public class InMemoryEventSourcedRepository<TId, TState> : IStartSessions<TId, TState>
    {
        private ConcurrentDictionary<TId, List<ItemWithType>> eventStore = new ConcurrentDictionary<TId, List<ItemWithType>>();
        private readonly IHoldAllConfiguration configuration;
        private static bool useThreadSafeOps;

        public ItemWithType[] this[TId id]
        {
            get => eventStore.TryGetValue(id, out var value) ? value.ToArray() : new ItemWithType[0];
            set => eventStore[id] = new List<ItemWithType>(value ?? new ItemWithType[0]);
        }

        public TId[] IdsOfStreamsWithEvents =>
            eventStore
                .Where(x => x.Value != null && x.Value.Count > 0)
                .Select(x => x.Key)
                .ToArray();

        public InMemoryEventSourcedRepository(IHoldAllConfiguration configuration)
        {
            this.configuration = configuration;
            useThreadSafeOps = configuration.ThreadSafetySelector(typeof(TState));
        }

        public Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            List<ItemWithType> eventStream;

            bool streamAlreadyExisted = eventStore.TryGetValue(id, out eventStream);

            if (!streamAlreadyExisted && !throwIfNotExists)
                eventStream = eventStore.GetOrAdd(id, new List<ItemWithType>());
            else if (!streamAlreadyExisted && throwIfNotExists)
                throw new StreamNotFoundException(id.ToString());

            lock (eventStream)
            {
                var session = new InMemoryEventStoreSession<TState, TId>(configuration, eventStream, id);
                session.LoadFromEvents(eventStream.ToArray(), eventStream.Count);

                return Task.FromResult((IManageSessionOf<TState>)session);
            }
        }

        public Task Delete(TId id)
        {
            if (eventStore.ContainsKey(id)) eventStore.TryRemove(id, out _);

            return Task.FromResult(0);
        }

        public Task<bool> Contains(TId id)
        {
            return Task.FromResult(eventStore.ContainsKey(id) && eventStore[id] != null && eventStore[id].Count > 0);
        }
    }
}