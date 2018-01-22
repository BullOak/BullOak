using BullOak.Repositories.Session;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BullOak.Repositories.EventStore
{
    public class EventStoreSession<TState> : BaseEventSourcedSession<TState, int>
    {
        private static readonly Task<int> done = Task.FromResult(0);

        public EventStoreSession(IHoldAllConfiguration configuration, )
            : base(configuration)
        {
        }

        public void Initialize()
        {
            //LoadFromEvents(eventStream.CommittedEvents.Select(x => x.Body).ToArray(), eventStream.StreamRevision);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { };
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
                //for (var index = 0; index < eventsToAdd.Length; index++)
                //{
                //    eventStream.Add(new EventMessage()
                //    {
                //        Body = eventsToAdd[index]
                //    });
                //}

                //eventStream.CommitChanges(Guid.NewGuid());

                //return eventStream.StreamRevision;
                return 0;
            }
            else throw new NotSupportedException("Snapshotting not yet supported.");
        }

    }
}
