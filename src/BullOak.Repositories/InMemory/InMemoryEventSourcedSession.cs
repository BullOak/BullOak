namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.Repositories.EventSourced;

    internal class InMemoryEventSourcedSession<TState> : EventSourceSession<TState, int>
        where TState : new()
    {
        private readonly Func<List<IHoldEventWithMetadata>, int, Task> store;

        public InMemoryEventSourcedSession(ICreateEventAppliers eventAppliersFactory,
            Func<List<IHoldEventWithMetadata>, int, Task> store)
            : base(eventAppliersFactory)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        protected override Task SaveEvents(List<IHoldEventWithMetadata> newEvents, int concurrency)
            => store(newEvents, concurrency);
    }
}
