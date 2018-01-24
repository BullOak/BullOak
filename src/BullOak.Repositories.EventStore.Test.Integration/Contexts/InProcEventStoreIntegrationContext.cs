namespace BullOak.Repositories.EventStore.Test.Integration.Contexts
{
    using BullOak.Repositories.EventStore.Test.Integration.Components;
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
        private ClusterVNode node;
        private EventStoreRepository<string, IHoldHigherOrder> repository;
        private IEventStoreConnection connection;

        private IEventStoreConnection CreateConnection()
        {
            if (connection == null)
                connection = EmbeddedEventStoreConnection.Create(node);
            return connection;
        }

        public void Setup(IHoldAllConfiguration configuration)
        {
            node = CreateInMemoryEventStoreNode();
            repository = new EventStoreRepository<string, IHoldHigherOrder>(configuration, CreateConnection());
        }

        public void Teardown()
        {
            node.Stop();
        }

        public async Task AppendEventsToStream(Guid id, MyEvent[] events)
        {
            using (var session = await repository.BeginSessionFor(id.ToString()))
            {
                session.AddEvents(events);
                await session.SaveChanges().ConfigureAwait(false);
            }
        }

        public ResolvedEvent[] ReadEventsFromStreamRaw(Guid id)
        {
            var connection = CreateConnection();
            var events = connection.ReadStreamEventsForwardAsync(id.ToString(), 0, 4096, false).Result;
            return events.Events;
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


        private ClusterVNode CreateInMemoryEventStoreNode()
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
