namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Repository;
    using BullOak.Repositories.Session;

    public class InMemoryEventSourcedRepository<TId, TState> : IStartSessions<TId, TState>
    {
        private Dictionary<TId, ItemWithType[]> eventStore = new Dictionary<TId, ItemWithType[]>();
        private readonly IHoldAllConfiguration configuration;
        private static bool useThreadSafeOps;

        public ItemWithType[] this[TId id]
        {
            get => eventStore.TryGetValue(id, out var value) ? value : new ItemWithType[0];
            set => eventStore[id] = value;
        }

        public TId[] IdsOfStreamsWithEvents =>
            eventStore
                .Where(x => x.Value != null && x.Value.Length > 0)
                .Select(x => x.Key)
                .ToArray();

        public InMemoryEventSourcedRepository(IHoldAllConfiguration configuration)
        {
            this.configuration = configuration;
            useThreadSafeOps = configuration.ThreadSafetySelector(typeof(TState));
        }

        public Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            ItemWithType[] eventStream;
            bool lockTaken = false;

            try
            {
                if (useThreadSafeOps) Monitor.TryEnter(eventStore, ref lockTaken);
                if (useThreadSafeOps && !lockTaken) throw new Exception("Lock not taken");

                if (!eventStore.TryGetValue(id, out eventStream) && !throwIfNotExists)
                {
                    eventStream = new ItemWithType[0];
                    eventStore[id] = eventStream;
                }
                else if (throwIfNotExists)
                    throw new StreamNotFoundException(id.ToString());
            }
            finally
            {
                if(lockTaken) Monitor.Exit(eventStore);
            }

            var session = new InMemoryEventStoreSession<TState, TId>(configuration, eventStore, id);

            try
            {
                lockTaken = false;
                if (useThreadSafeOps) Monitor.TryEnter(eventStore, ref lockTaken);
                if (useThreadSafeOps && !lockTaken) throw new Exception("Lock not taken");

                session.LoadFromEvents(eventStore[id].Select(x=>x.instance).ToArray(), eventStore[id].Length);
            }
            finally
            {
                if(lockTaken) Monitor.Exit(eventStore);
            }

            return Task.FromResult((IManageSessionOf<TState>)session);
        }

        public Task Delete(TId id)
        {
            lock (eventStore)
            {
                if (eventStore.ContainsKey(id)) eventStore.Remove(id);
                return Task.FromResult(0);
            }
        }

        public Task<bool> Contains(TId id)
        {
            lock (eventStore)
            {
                return Task.FromResult(eventStore.ContainsKey(id) && eventStore[id] != null && eventStore[id].Length > 0);
            }
        }
    }
}