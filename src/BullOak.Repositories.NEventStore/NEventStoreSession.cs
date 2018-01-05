//namespace BullOak.Repositories.NEventStore
//{
//    using System;
//    using System.Threading.Tasks;
//    using BullOak.Repositories.Session;
//    using global::NEventStore;

//    internal class NEventStoreSession<TState> : RepoSessionWithConcurrency<TState, int>
//    {
//        private readonly IEventStream eventStream;
//        private static readonly Task Done = Task.FromResult(0);

//        public NEventStoreSession(IHoldAllConfiguration configuration, IEventStream stream)
//            : base(configuration)
//        {
//            eventStream = stream ?? throw new ArgumentNullException(nameof(stream));

//            LoadFromEvents(stream.CommittedEvents, stream.StreamRevision);
//        }

//        protected override Task SaveChangesProtected(object[] newEvents, TState latestState, int concurrencyId, bool eventsAlreadySent)
//        {
//            for (var index = 0; index < newEvents.Length; index++)
//            {
//                eventStream.Add(new EventMessage()
//                {
//                    Body = newEvents[index]
//                });
//            }

//            eventStream.CommitChanges(Guid.NewGuid());

//            return Done;
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if(disposing) eventStream.Dispose();
//            base.Dispose(disposing);
//        }
//    }
//}
