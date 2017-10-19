namespace BullOak.Repositories.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.Repositories.EventSourced;

    internal class InMemoryEventSourcedSession<TState> : EventSourceSession<TState, int>
        where TState : new()
    {
        private readonly Func<List<object>, int, Task> store;

        public InMemoryEventSourcedSession(IEnumerable<IApplyEvents<TState>> eventAppliers,
            Func<List<object>, int, Task> store)
            : base(eventAppliers)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        protected override Task SaveEvents(List<object> newEvents, int concurrency)
            => store(newEvents, concurrency);
    }
}
