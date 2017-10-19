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

        public NEventStoreSession(IEnumerable<IApplyEvents<TState>> appliers, IEventStream stream)
            : base(appliers)
        {
            eventStream = stream;
        }

        protected override Task SaveEvents(List<object> newEvents, int concurrency)
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
