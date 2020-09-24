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
        private ConcurrentDictionary<TId, List<(ItemWithType, DateTime)>> eventStore = new ConcurrentDictionary<TId, List<(ItemWithType, DateTime)>>();
        private readonly IHoldAllConfiguration configuration;
        private static bool useThreadSafeOps;
        private readonly IValidateState<TState> stateValidator = new AlwaysPassValidator<TState>();
        public IValidateState<TState> StateValidator => stateValidator;

        public ItemWithType[] this[TId id]
        {
            get => eventStore.TryGetValue(id, out var value) ? value.Select(x => x.Item1).ToArray() : new ItemWithType[0];
            set => eventStore[id] = new List<(ItemWithType, DateTime)>((value ?? new ItemWithType[0]).Select(x => (x, DateTime.UtcNow)));
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

        public InMemoryEventSourcedRepository(IValidateState<TState> stateValidator, IHoldAllConfiguration configuration)
            : this(configuration)
        {
            this.stateValidator = stateValidator;
        }

        public Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false, DateTime? upTo = null)
        {
            if (!eventStore.TryGetValue(id, out var eventStream))
            {
                if (throwIfNotExists) throw new StreamNotFoundException(id.ToString());
                eventStream = eventStore.GetOrAdd(id, new List<(ItemWithType, DateTime)>());
            }

            lock (eventStream)
            {
                var session = stateValidator == null
                    ? new InMemoryEventStoreSession<TState, TId>(configuration, eventStream, id)
                    : new InMemoryEventStoreSession<TState, TId>(stateValidator, configuration, eventStream, id);

                var streamData = eventStream
                    .TakeWhile(x => !upTo.HasValue || x.Item2 <= upTo.Value)
                    .Select(x => x.Item1)
                    .ToArray();

                session.LoadFromEvents(streamData, eventStream.Count);

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
