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
        private readonly TId Id;
        private readonly int initialVersion;
        private readonly List<ItemWithType> stream;

        public InMemoryEventStoreSession(IHoldAllConfiguration configuration, List<ItemWithType> stream, TId id)
            : base(configuration)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            initialVersion = stream.Count;
            Id = id;
        }

        /// <inheritdoc />
        protected override Task<int> SaveChanges(ItemWithType[] newEvents,
            TState currentState,
            CancellationToken? cancellationToken)
        {
            lock (stream)
            {
                if (stream.Count != initialVersion)
                    throw new ConcurrencyException(Id.ToString(), null);

                stream.AddRange(newEvents ?? new ItemWithType[0]);

                return Task.FromResult(stream.Count);
            }
        }
    }
}