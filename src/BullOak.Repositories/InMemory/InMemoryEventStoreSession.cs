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

        /// <inheritdoc />
        protected override Task<int> SaveChanges(object[] newEvents, TState currentState, CancellationToken? cancellationToken)
        {
            lock (data)
            {
                if (data.ContainsKey(id))
                {
                    var events = data[id].ToList();
                    events.AddRange(newEvents);

                    data[id] = events.ToArray();
                }
                else data[id] = newEvents;

                return Task.FromResult(data[id].Length);
            }
        }
    }
}