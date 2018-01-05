namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.Session;
    using global::NEventStore;

    internal class NEventStoreSession<TState> : BaseEventSourcedSyncSession<TState, int>
    {
        private readonly IEventStream eventStream;

        public NEventStoreSession(IHoldAllConfiguration configuration, IEventStream stream)
            : base(configuration)
        {
            eventStream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public void Initialize()
        {
            LoadFromEvents(eventStream.CommittedEvents.Select(x=> x.Body).ToArray(), eventStream.StreamRevision);
        }

        protected override void SaveChanges(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot)
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
            }
            else throw new NotSupportedException("Snapshotting not yet supported.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) eventStream.Dispose();
            base.Dispose(disposing);
        }
    }
}
