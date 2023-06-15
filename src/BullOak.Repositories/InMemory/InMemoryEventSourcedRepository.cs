namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Repository;
    using BullOak.Repositories.Session;

    public class InMemoryEventSourcedRepository<TId, TState> : IStartSessions<TId, TState>
    {
        private ConcurrentDictionary<TId, List<(StoredEvent, DateTime)>> eventStore = new ConcurrentDictionary<TId, List<(StoredEvent, DateTime)>>();
        private readonly IHoldAllConfiguration configuration;
        private static bool useThreadSafeOps;
        private readonly IValidateState<TState> stateValidator = new AlwaysPassValidator<TState>();
        public readonly bool IsLoadedAsynchronously;
        public IValidateState<TState> StateValidator => stateValidator;

        public (StoredEvent, DateTime)[] this[TId id]
        {
            get => eventStore.TryGetValue(id, out var value) ? value.ToArray() : new (StoredEvent, DateTime)[0];
            set => eventStore[id] = new List<(StoredEvent, DateTime)>(value ?? new (StoredEvent, DateTime)[0]);
        }

        public TId[] IdsOfStreamsWithEvents =>
            eventStore
                .Where(x => x.Value != null && x.Value.Count > 0)
                .Select(x => x.Key)
                .ToArray();

        public InMemoryEventSourcedRepository(IHoldAllConfiguration configuration, bool loadAsynchronously = false)
        {
            this.configuration = configuration;
            useThreadSafeOps = configuration.ThreadSafetySelector(typeof(TState));
            IsLoadedAsynchronously = loadAsynchronously;
        }

        public InMemoryEventSourcedRepository(IValidateState<TState> stateValidator, IHoldAllConfiguration configuration, bool loadAsynchronously = false)
            : this(configuration, loadAsynchronously)
        {
            this.stateValidator = stateValidator;
        }

        public Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false, DateTime? appliesAt = null)
        {
            if (!eventStore.TryGetValue(id, out var eventStream))
            {
                if (throwIfNotExists) throw new StreamNotFoundException(id.ToString());
                eventStream = eventStore.GetOrAdd(id, new List<(StoredEvent, DateTime)>());
            }

            lock (eventStream)
            {
                var session = stateValidator == null
                    ? new InMemoryEventStoreSession<TState, TId>(configuration, eventStream, id)
                    : new InMemoryEventStoreSession<TState, TId>(stateValidator, configuration, eventStream, id);

                var streamData = eventStream
                    .TakeWhile(x => !appliesAt.HasValue || x.Item2 <= appliesAt.Value)
                    .Select(x => x.Item1)
                    .ToArray();

                if (IsLoadedAsynchronously)
                    return LoadAsyncAndReturnSession(session, streamData);
                else
                {
                    session.LoadFromEvents(streamData);

                    return Task.FromResult((IManageSessionOf<TState>)session);
                }
            }
        }

        private async Task<IManageSessionOf<TState>> LoadAsyncAndReturnSession(InMemoryEventStoreSession<TState, TId> inMemSession, StoredEvent[] events)
        {
            await inMemSession.LoadFromEvents(ToAsyncEnumerable(events));

            return inMemSession;
        }

        private async IAsyncEnumerable<StoredEvent> ToAsyncEnumerable(StoredEvent[] streamData)
        {
            for (int i = 0; i < streamData.Length; i++)
                yield return streamData[i];

            await Task.CompletedTask;
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
