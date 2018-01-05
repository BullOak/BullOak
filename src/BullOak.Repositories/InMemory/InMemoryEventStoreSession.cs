namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.Session;

    internal class InMemoryEventStoreSession<TState, TId> : BaseEventSourcedSyncSession<TState, int>
    {
        private readonly Dictionary<TId, object[]> data;
        private readonly TId id;

        public InMemoryEventStoreSession(IHoldAllConfiguration configuration, Dictionary<TId, object[]> data, TId id)
            : base(configuration, null)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.id = id;
        }

        protected override void SaveChanges(object[] eventsToAdd,
            bool shouldSaveSnapshot,
            TState snapshot)
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
            }
        }
    }
}