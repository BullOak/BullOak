namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Session;
    using global::NEventStore;

    internal class NEventStoreSession<TState> : BaseEventSourcedSession<TState, int>
    {
        private readonly IEventStream eventStream;
        private static readonly Task<int> done = Task.FromResult(0);

        public NEventStoreSession(IHoldAllConfiguration configuration, IEventStream stream)
            : base(configuration)
        {
            eventStream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public void Initialize()
        {
            LoadFromEvents(eventStream.CommittedEvents.Select(x=>
            {
                Type eventType = x.Headers.TryGetValue("EventType", out object t)
                    ? (Type) t
                    : x.Body.GetType();

                return new ItemWithType(x.Body, eventType);
            }).ToArray(), eventStream.StreamRevision);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) eventStream.Dispose();
            base.Dispose(disposing);
        }

        protected override Task<int> SaveChanges(ItemWithType[] newEvents,
            TState currentState,
            CancellationToken? cancellationToken)
        {
            for (var index = 0; index < newEvents.Length; index++)
            {
                eventStream.Add(new EventMessage()
                {
                    Body = newEvents[index].instance,
                    Headers = new Dictionary<string, object>
                    {
                        { "EventType", newEvents[index].type }
                    }
                });
            }

            try
            {
                eventStream.CommitChanges(Guid.NewGuid());
            }

            catch (ConcurrencyException nce)
            {
                throw new Exceptions.ConcurrencyException(eventStream.StreamId, nce);
            }

            return Task.FromResult(eventStream.StreamRevision);
        }
    }
}
