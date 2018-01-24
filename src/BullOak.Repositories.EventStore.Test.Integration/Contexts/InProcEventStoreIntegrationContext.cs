namespace BullOak.Repositories.EventStore.Test.Integration.Contexts
{
    using BullOak.Repositories.EventStore.Test.Integration.Components;
    using BullOak.Repositories.Session;
    using global::EventStore.ClientAPI;
    using global::EventStore.ClientAPI.Embedded;
    using global::EventStore.Core;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class InProcEventStoreIntegrationContext
    {
        private static ClusterVNode node;
        private EventStoreRepository<string, IHoldHigherOrder> repository;
        private IEventStoreConnection connection;

        private IEventStoreConnection CreateConnection()
        {
            if (connection == null)
                connection = EmbeddedEventStoreConnection.Create(node);
            return connection;
        }

        public void SetupRepository(IHoldAllConfiguration configuration)
        {
            repository = new EventStoreRepository<string, IHoldHigherOrder>(configuration, CreateConnection());
        }

        public static void SetupNode()
        {
            node = CreateInMemoryEventStoreNode();
        }

        public static void TeardownNode()
        {
            node.Stop();
        }

        public async Task<IManageAndSaveSessionWithSnapshot<IHoldHigherOrder>> StartSession(Guid currentStreamId)
        {
            var session = await repository.BeginSessionFor(currentStreamId.ToString());
            return session;

        }

        public async Task AppendEventsToCurrentStream(Guid id, MyEvent[] events)
        {
            using (var session = await StartSession(id))
            {
                session.AddEvents(events);
                await session.SaveChanges().ConfigureAwait(false);
            }
        }

        public ResolvedEvent[] ReadEventsFromStreamRaw(Guid id)
        {
            var connection = CreateConnection();
            var result = new List<ResolvedEvent>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = connection.ReadStreamEventsForwardAsync(id.ToString(), nextSliceStart, 100, false).Result;
                nextSliceStart = currentSlice.NextEventNumber;
                result.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            return result.ToArray();
        }

        internal void WriteEventsToStreamRaw(Guid currentStreamInUse, IEnumerable<MyEvent> myEvents)
        {
            var connection = CreateConnection();
            connection.AppendToStreamAsync(currentStreamInUse.ToString(), ExpectedVersion.Any,
                myEvents.Select(e =>
                {
                    var serialized = JsonConvert.SerializeObject(e);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(serialized);
                    return new EventData(Guid.NewGuid(),
                        e.GetType().AssemblyQualifiedName,
                        true,
                        bytes,
                        null);
                }))
                .Wait();
        }

        private static ClusterVNode CreateInMemoryEventStoreNode()
        {
            var nodeBuilder = EmbeddedVNodeBuilder.AsSingleNode()
                                      .OnDefaultEndpoints()
                                      .RunInMemory();
            var node = nodeBuilder.Build();
            node.StartAndWaitUntilReady().Wait();
            return node;
        }


    }
}
