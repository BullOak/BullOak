using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;

namespace BullOak.Repositories.EventStore.Test.Integration.Contexts
{
    class EventStoreContainer
    {
        private ClusterVNode node;

        public IEventStoreConnection CreateConnection()
        {
            return EmbeddedEventStoreConnection.Create(node);
        }

        public void Setup()
        {
            var nodeBuilder = EmbeddedVNodeBuilder.AsSingleNode()
                                      .OnDefaultEndpoints()
                                      .RunInMemory();
            node = nodeBuilder.Build();
            node.StartAndWaitUntilReady().Wait();
        }
    }
}
