namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Common;
    using BullOak.Common.Exceptions;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Session;

    public class InMemoryEventSourcedRepository<TState, TId> : IManagePersistenceOf<TState, TId>
    {
        private static readonly Task done = Task.FromResult(0);
        private Dictionary<TId, object[]> eventStore = new Dictionary<TId, object[]>();
        private readonly IHoldAllConfiguration configuration;

        public object[] this[TId id]
        {
            get
            {
                if(eventStore.TryGetValue(id, out var value)) return value;
                return new object[0];
            }
            set => eventStore[id] = value;
        }

        public InMemoryEventSourcedRepository(IHoldAllConfiguration configuration)
            => this.configuration = configuration;

        public Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            object[] eventStream;

            lock (eventStore)
            {
                if (!eventStore.TryGetValue(id, out eventStream) && !throwIfNotExists)
                {
                    eventStream = new object[0];
                    eventStore[id] = eventStream;
                }
                else if (throwIfNotExists) throw new StreamNotFoundException(id.ToString());
            }

            var session = new BasicEventSourcedRepoSession<TState>(configuration, 
                (e, c) => SaveEvents(id, e, c));

            lock (eventStore)
            {
                session.LoadFromEvents(eventStore[id], eventStore[id].Length);
            }

            return Task.FromResult((IManageSessionOf<TState>) session);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private Task SaveEvents(TId id, object[] newEvents, int concurrencyId)
        {
            var list = eventStore[id];

            if(list.Length != concurrencyId) throw new ConcurrencyException(id.ToString(), typeof(TState));

            var newBuffer = new object[list.Length + newEvents.Length];
            Array.Copy(list, 0, newBuffer, 0, list.Length);
            Array.Copy(newEvents, 0, newBuffer, list.Length, newEvents.Length);
            eventStore[id] = newBuffer;

            return done;
        }

        public Task Clear(TId id)
        {
            if(eventStore.ContainsKey(id)) eventStore[id] = new object[0];

            return done;
        }

        public Task<bool> Exists(TId id)
            => Task.FromResult(eventStore.ContainsKey(id));
    }
}
