namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventSourced;
    using global::NEventStore;

    internal class NEventStoreSession<TState> : EventSourceSession<TState, int>
        where TState : new()
    {
        private readonly IEventStream eventStream;
        private static readonly Task Done = Task.FromResult(0);

        public NEventStoreSession(IApplyEvents<TState>[] appliers, IEventStream stream)
            : base(appliers)
        {
            eventStream = stream;
        }

        protected override Task SaveEvents(object[] newEvents, int concurrency)
        {
            for (var index = 0; index < newEvents.Length; index++)
            {
                eventStream.Add(new EventMessage()
                {
                    Body = newEvents[index]
                });
            }

            eventStream.CommitChanges(Guid.NewGuid());

            return Done;
        }
    }
}
