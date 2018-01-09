namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Session;

    internal class InMemoryEventStoreSession<TState, TId> : BaseEventSourcedSession<TState, int>
    {
        private readonly Dictionary<TId, object[]> data;
        private readonly TId id;
        private static readonly Task<int> done = Task.FromResult(0);

        public InMemoryEventStoreSession(IHoldAllConfiguration configuration, Dictionary<TId, object[]> data, TId id)
            : base(configuration, null)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.id = id;
        }

        protected override Task<int> SaveChanges(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot, CancellationToken? cancellationToken)
        {
            SaveChangesSync(eventsToAdd, shouldSaveSnapshot, snapshot);

            return done;
        }

        protected override int SaveChangesSync(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot)
        {
            if (shouldSaveSnapshot)
                throw new ArgumentException("Saving snapshots is not supported.");

            lock (data)
            {
                if (data.ContainsKey(id))
                {
                    var events = data[id].ToList();
                    events.AddRange(eventsToAdd);

                    data[id] = events.ToArray();
                }
                else data[id] = eventsToAdd;

                return data[id].Length;
            }
        }
    }
}