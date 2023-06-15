namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Session;

    internal class InMemoryEventStoreSession<TState, TId> : BaseEventSourcedSession<TState>
    {
        private readonly TId Id;
        private readonly int initialVersion;
        private readonly List<(StoredEvent, DateTime)> stream;

        public InMemoryEventStoreSession(IValidateState<TState> stateValidator, IHoldAllConfiguration configuration, List<(StoredEvent, DateTime)> stream, TId id)
            : base(stateValidator, configuration)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            initialVersion = stream.Count;
            Id = id;
        }

        public InMemoryEventStoreSession(IHoldAllConfiguration configuration, List<(StoredEvent, DateTime)> stream, TId id)
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

                if(newEvents == null)
                    newEvents = new ItemWithType[0];
                var count = stream.Count;

                foreach (var newEvent in newEvents)
                    stream.Add((StoredEvent.FromItemWithType(newEvent, count++), DateTime.Now));

                return Task.FromResult(stream.Count);
            }
        }
    }
}
