

namespace BullOak.Repositories.EventStore
{
    using BullOak.Repositories.Session;
    using global::EventStore.ClientAPI;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    public class EventStoreSession<TState> : BaseEventSourcedSession<TState, int>
    {
        private static readonly Task<int> done = Task.FromResult(0);
        private readonly IEventStoreConnection eventStoreConnection;
        private readonly string streamName;
        private int currentVersion;

        public EventStoreSession(IHoldAllConfiguration configuration,
                                 IEventStoreConnection eventStoreConnection,
                                 string streamName)
            : base(configuration)
        {
            this.eventStoreConnection = eventStoreConnection ?? throw new ArgumentNullException(nameof(eventStoreConnection));
            this.streamName = streamName ?? throw new ArgumentNullException(nameof(streamName));
        }

        public async Task Initialize()
        {
            //TODO: user credentials
            //TODO: paged read instead of 4096
            var events = await eventStoreConnection.ReadStreamEventsForwardAsync(streamName, 0, 4096, false).ConfigureAwait(false);
            currentVersion = (int)events.LastEventNumber;

            LoadFromEvents(events.Events.Select(resolvedEvent =>
                            {
                                return GetEventFromEventData(resolvedEvent);
                            }).ToArray(),
                            currentVersion);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) { };
            base.Dispose(disposing);
        }

        /// <summary>
        /// Saves changes to the respective stream
        /// NOTES: Current implementation doesn't support cancellation token
        /// </summary>
        /// <param name="eventsToAdd"></param>
        /// <param name="shouldSaveSnapshot"></param>
        /// <param name="snapshot"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<int> SaveChanges(object[] eventsToAdd,
            bool shouldSaveSnapshot,
            TState snapshot,
            CancellationToken? cancellationToken)
        {
            checked
            {
                var writeResult = await eventStoreConnection.ConditionalAppendToStreamAsync(
                    streamName,
                    currentVersion,
                    eventsToAdd.Select(eventObject =>
                    {
                        return CreateEventData(eventObject);
                    }));

                switch (writeResult.Status)
                {
                    case ConditionalWriteStatus.Succeeded:
                        break;
                    case ConditionalWriteStatus.VersionMismatch:
                        throw new InvalidOperationException($"Stream expected version mismatched actual version. StreamId: {streamName}");
                    case ConditionalWriteStatus.StreamDeleted:
                        throw new InvalidOperationException($"Stream was deleted. StreamId: {streamName}");
                    default:
                        throw new InvalidOperationException($"Unexpected write result: {writeResult.Status}");
                }

                return writeResult.NextExpectedVersion.HasValue ?
                    (int)writeResult.NextExpectedVersion.Value :
                    throw new InvalidOperationException("Eventstore data write outcome unexpected. NextExpectedVersion is null");
            }
        }

        private EventData CreateEventData(object eventObject)
        {
            var eventData = new EventData(
                Guid.NewGuid(),
                eventObject.GetType().AssemblyQualifiedName,
                true,
                System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventObject)),
                null);
            return eventData;
        }

        private object GetEventFromEventData(ResolvedEvent resolvedEvent)
        {
            var result = JsonConvert.DeserializeObject(
                System.Text.Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                Type.GetType(resolvedEvent.Event.EventType));

            return result;
        }


        private async Task<int> SaveChangesWithConfiguredAwait(object[] eventsToAdd,
            bool shouldSaveSnapshot,
            TState snapshot)
        {
            return await SaveChanges(eventsToAdd, shouldSaveSnapshot, snapshot, null).ConfigureAwait(false);
        }

        protected override int SaveChangesSync(object[] eventsToAdd, bool shouldSaveSnapshot, TState snapshot)
        {
            if (shouldSaveSnapshot) throw new NotSupportedException("Snapshotting not yet supported.");
            return SaveChangesWithConfiguredAwait(eventsToAdd, shouldSaveSnapshot, snapshot).Result;
        }

    }
}
