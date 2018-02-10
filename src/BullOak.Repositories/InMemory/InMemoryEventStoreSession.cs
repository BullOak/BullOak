namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Session;

    internal class InMemoryEventStoreSession<TState, TId> : BaseEventSourcedSession<TState, int>
    {
        private readonly int initialVersion;
        private readonly Dictionary<TId, object[]> data;
        private readonly TId id;

        public InMemoryEventStoreSession(IHoldAllConfiguration configuration, Dictionary<TId, object[]> data, TId id)
            : base(configuration)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            initialVersion = data[id].Length;
            this.id = id;
        }

        /// <inheritdoc />
        protected override Task<int> SaveChanges(object[] newEvents, TState currentState, CancellationToken? cancellationToken)
        {
            lock (data)
            {
                if (data.ContainsKey(id))
                {
                    var events = data[id].ToList();

                    if (events.Count != initialVersion)
                        throw new ConcurrencyException(id.ToString(), null);

                    events.AddRange(newEvents);

                    data[id] = events.ToArray();
                }
                else data[id] = newEvents;

                return Task.FromResult(data[id].Length);
            }
        }
    }
}