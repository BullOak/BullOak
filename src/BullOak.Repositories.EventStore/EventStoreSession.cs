namespace BullOak.Repositories.EventStore
{
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Session;
    using BullOak.Repositories.StateEmit;
    using global::EventStore.ClientAPI;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class EventStoreSession<TState> : BaseEventSourcedSession<TState, int>
    {
        private static readonly Task<int> done = Task.FromResult(0);
        private readonly IEventStoreConnection eventStoreConnection;
        private readonly string streamName;
        private int currentVersion;
        private bool isInDisposedState = false;

        private int SliceSize { get; set; } = 1024; //4095 is max allowed value

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
            CheckDisposedState();
            //TODO: user credentials
            var events = await ReadAllEventsFromStream();
            LoadFromEvents(events.Select(resolvedEvent =>
                            {
                                return GetEventFromEventData(resolvedEvent);
                            }).ToArray(),
                            currentVersion);

        }

        private void CheckDisposedState()
        {
            if (isInDisposedState)
            {
                //this is purely design decision, nothing prevents implementing the session that support any amount and any order of oeprations
                throw new InvalidOperationException("EventStoreSession should not be used after SaveChanges call");
            }
        }

        private async Task<List<ResolvedEvent>> ReadAllEventsFromStream()
        {
            checked
            {
                var result = new List<ResolvedEvent>();
                StreamEventsSlice currentSlice;
                long nextSliceStart = StreamPosition.Start;
                do
                {
                    currentSlice = await eventStoreConnection.ReadStreamEventsForwardAsync(streamName, nextSliceStart, SliceSize, false);
                    if (currentSlice.Status == SliceReadStatus.StreamDeleted ||
                        currentSlice.Status == SliceReadStatus.StreamNotFound)
                    {
                        currentVersion = -1;
                        return result;
                    }
                    nextSliceStart = currentSlice.NextEventNumber;
                    result.AddRange(currentSlice.Events);
                } while (!currentSlice.IsEndOfStream);
                currentVersion = (int)currentSlice.LastEventNumber;
                return result;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { ConsiderSessionDisposed(); }
            base.Dispose(disposing);
        }

        private void ConsiderSessionDisposed()
        {
            isInDisposedState = true;
        }

        /// <summary>
        /// Saves changes to the respective stream
        /// NOTES: Current implementation doesn't support cancellation token
        /// </summary>
        /// <param name="eventsToAdd"></param>
        /// <param name="snapshot"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<int> SaveChanges(ItemWithType[] eventsToAdd,
            TState snapshot,
            CancellationToken? cancellationToken)
        {
            checked
            {
                CheckDisposedState();
                ConditionalWriteResult writeResult;

                writeResult = await eventStoreConnection.ConditionalAppendToStreamAsync(
                        streamName,
                        currentVersion,
                        eventsToAdd.Select(eventObject => CreateEventData(eventObject)))
                    .ConfigureAwait(false);

                switch (writeResult.Status)
                {
                    case ConditionalWriteStatus.Succeeded:
                        break;
                    case ConditionalWriteStatus.VersionMismatch:
                        throw new ConcurrencyException(streamName, null);
                    case ConditionalWriteStatus.StreamDeleted:
                        throw new InvalidOperationException($"Stream was deleted. StreamId: {streamName}");
                    default:
                        throw new InvalidOperationException($"Unexpected write result: {writeResult.Status}");
                }

                if (!writeResult.NextExpectedVersion.HasValue)
                {
                    throw new InvalidOperationException("Eventstore data write outcome unexpected. NextExpectedVersion is null");
                }

                ConsiderSessionDisposed();
                currentVersion = (int)writeResult.NextExpectedVersion.Value;
                return (int)writeResult.NextExpectedVersion.Value;
            }
        }

        private EventData CreateEventData(ItemWithType @event)
        {
            var metadata = EventMetadata_V1.From(@event);

            var eventData = new EventData(
                Guid.NewGuid(),
                @event.type.Name,
                true,
                System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event.instance)),
                MetadataSerializer.Serialize(metadata));
            return eventData;
        }

        private ItemWithType GetEventFromEventData(ResolvedEvent resolvedEvent)
        {
            var jobject = JObject.Parse(System.Text.Encoding.UTF8.GetString(resolvedEvent.Event.Data));
            Type type;
            (IHoldMetadata metadata,int version) metadata;

            if (resolvedEvent.Event.Metadata == null || resolvedEvent.Event.Metadata.Length == 0)
            {
                type = Type.GetType(resolvedEvent.Event.EventType);
            }
            else
            {
                metadata = MetadataSerializer.DeserializeMetadata(resolvedEvent.Event.Metadata);
                type = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(x => x.GetType(metadata.metadata.EventTypeFQN))
                    .FirstOrDefault(x => x != null);
            }

            object @event;
            if (type.IsInterface)
            {
                @event = configuration.StateFactory.GetState(type);
                var switchable = @event as ICanSwitchBackAndToReadOnly;

                var canEdit = jobject.Property("canEdit");
                canEdit.Remove();

                switchable.CanEdit = true;
                var reader = jobject.CreateReader();
                var serializer = new JsonSerializer();
                serializer.Populate(reader, @event);
                switchable.CanEdit = false;
            }
            else
                @event = jobject.ToObject(type);

            return new ItemWithType(@event, type);
        }
    }
}
