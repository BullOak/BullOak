namespace BullOak.Repositories.EventStore.Test.Integration.Contexts
{
    using BullOak.Repositories.EventStore.Test.Integration.Components;
    using global::EventStore.ClientAPI;
    using global::EventStore.ClientAPI.Embedded;
    using global::EventStore.Core;
    using System;
    using System.Threading.Tasks;

    internal class InProcEventStoreIntegrationContext
    {
        private ClusterVNode node;
        private EventStoreRepository<string, IHoldHigherOrder> repository;
        private IEventStoreConnection connection;

        private IEventStoreConnection CreateConnection(ClusterVNode node)
        {
            if (connection == null)
                connection = EmbeddedEventStoreConnection.Create(node);
            return connection;
        }

        public void Setup(IHoldAllConfiguration configuration)
        {
            node = CreateInMemoryEventStoreNode();
            repository = new EventStoreRepository<string, IHoldHigherOrder>(configuration, CreateConnection(node));
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
            var connection = CreateConnection(node);
            var events = connection.ReadStreamEventsForwardAsync(id.ToString(), 0, 4096, false).Result;
            return events.Events;
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
