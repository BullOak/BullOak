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
        public readonly bool IsLoadedAsynchronously;
        public IValidateState<TState> StateValidator => stateValidator;

        public (ItemWithType, DateTime)[] this[TId id]
        {
            get => eventStore.TryGetValue(id, out var value) ? value.ToArray() : new (ItemWithType, DateTime)[0];
            set => eventStore[id] = new List<(ItemWithType, DateTime)>(value ?? new (ItemWithType,DateTime)[0]);
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
                eventStream = eventStore.GetOrAdd(id, new List<(ItemWithType, DateTime)>());
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
                    session.LoadFromEvents(streamData, eventStream.Count);

                    return Task.FromResult((IManageSessionOf<TState>)session);
                }
            }
        }

        private async Task<IManageSessionOf<TState>> LoadAsyncAndReturnSession(InMemoryEventStoreSession<TState, TId> inMemSession, ItemWithType[] events)
        {
            await inMemSession.LoadFromEvents(ToAsyncEnumerable(events), events.Length == 0, events.Length);

            return inMemSession;
        }

        private async IAsyncEnumerable<ItemWithType> ToAsyncEnumerable(ItemWithType[] streamData)
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
