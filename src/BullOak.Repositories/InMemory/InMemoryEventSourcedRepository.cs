namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Session;

    public class InMemoryEventSourcedRepository<TId, TState>
    {
        private Dictionary<TId, object[]> eventStore = new Dictionary<TId, object[]>();
        private readonly IHoldAllConfiguration configuration;
        private static bool useThreadSafeOps;

        public object[] this[TId id]
        {
            get => eventStore.TryGetValue(id, out var value) ? value : new object[0];
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

        public IManageAndSaveSession<TState> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            object[] eventStream;
            bool lockTaken = false;

            try
            {
                if (useThreadSafeOps) Monitor.TryEnter(eventStore, ref lockTaken);
                if (useThreadSafeOps && !lockTaken) throw new Exception("Lock not taken");

                if (!eventStore.TryGetValue(id, out eventStream) && !throwIfNotExists)
                {
                    eventStream = new object[0];
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

                session.LoadFromEvents(eventStore[id], eventStore[id].Length);
            }
            finally
            {
                if(lockTaken) Monitor.Exit(eventStore);
            }

            return session;
        }

        public void Clear(TId id)
        {
            lock (eventStore)
            {
                if (eventStore.ContainsKey(id)) eventStore.Remove(id);
            }
        }

        public bool Exists(TId id)
        {
            lock (eventStore)
            {
                return eventStore.ContainsKey(id) && eventStore[id] != null && eventStore[id].Length > 0;
            }
        }
    }
}