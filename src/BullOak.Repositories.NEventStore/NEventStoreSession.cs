namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.Repositories.EventSourced;
    using global::NEventStore;

    internal class NEventStoreSession<TState> : EventSourceSession<TState, int>
        where TState : new()
    {
        private readonly IEventStream eventStream;
        private static readonly Task Done = Task.FromResult(0);

        public NEventStoreSession(ICreateEventAppliers factory, IEventStream stream)
            : base(factory)
        {
            eventStream = stream;
        }

        protected override Task SaveEvents(List<IHoldEventWithMetadata> newEvents, int concurrency)
        {
            foreach (var @event in newEvents)
            {
                eventStream.Add(new EventMessage()
                {
                    Body = @event
                });
            }

            eventStream.CommitChanges(Guid.NewGuid());

            return Done;
        }
    }
}
