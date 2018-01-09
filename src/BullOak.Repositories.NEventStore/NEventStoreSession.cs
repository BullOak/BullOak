namespace BullOak.Repositories.NEventStore
{
    using System;
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
            LoadFromEvents(eventStream.CommittedEvents.Select(x=> x.Body).ToArray(), eventStream.StreamRevision);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) eventStream.Dispose();
            base.Dispose(disposing);
        }

        protected override Task<int> SaveChanges(object[] eventsToAdd,
            bool shouldSaveSnapshot,
            TState snapshot,
            CancellationToken? cancellationToken)
            => Task.FromResult(SaveChangesSync(eventsToAdd, shouldSaveSnapshot, snapshot));

        protected override int SaveChangesSync(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot)
        {
            if (!shouldSaveSnapshot)
            {
                for (var index = 0; index < eventsToAdd.Length; index++)
                {
                    eventStream.Add(new EventMessage()
                    {
                        Body = eventsToAdd[index]
                    });
                }

                eventStream.CommitChanges(Guid.NewGuid());

                return eventStream.StreamRevision;
            }
            else throw new NotSupportedException("Snapshotting not yet supported.");
        }
    }
}
