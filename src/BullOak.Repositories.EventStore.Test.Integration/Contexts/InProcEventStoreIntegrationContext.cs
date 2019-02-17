namespace BullOak.Repositories.EventStore.Test.Integration.Contexts
{
    using BullOak.Repositories.Config;
    using BullOak.Repositories.EventStore.Test.Integration.Components;
    using BullOak.Repositories.Session;
    using global::EventStore.ClientAPI;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using BullOak.Repositories.EventStore.Test.Integration.EventStoreServer;
    using TechTalk.SpecFlow;

    internal class InProcEventStoreIntegrationContext
    {
        //private static ClusterVNode node;
        private EventStoreRepository<string, IHoldHigherOrder> repository;
        private static IEventStoreConnection connection;
        private static Process eventStoreProcess;

        public InProcEventStoreIntegrationContext()
        {
            var configuration = Configuration.Begin()
               .WithDefaultCollection()
               .WithDefaultStateFactory()
               .NeverUseThreadSafe()
               .WithNoEventPublisher()
               .WithAnyAppliersFrom(Assembly.GetExecutingAssembly())
               //.WithEventApplier(new StateApplier())
               .AndNoMoreAppliers()
               .WithNoUpconverters()
               .Build();

            SetupRepository(configuration);
        }

        private static IEventStoreConnection GetConnection()
        {
            return connection;
        }

        public void SetupRepository(IHoldAllConfiguration configuration)
        {
            repository = new EventStoreRepository<string, IHoldHigherOrder>(configuration, GetConnection());
        }

        [BeforeTestRun]
        public static async Task SetupNode()
        {
            if (connection == null)
            {
                RunEventStoreServerProcess();

                var settings = ConnectionSettings
                    .Create()
                    .KeepReconnecting()
                    .FailOnNoServerResponse()
                    .KeepRetrying()
                    .UseConsoleLogger();

                var localhostConnectionString = "ConnectTo=tcp://localhost:1113; HeartBeatTimeout=500";

                connection = EventStoreConnection.Create(localhostConnectionString, settings);
                await connection.ConnectAsync();
            }
        }

        private static void RunEventStoreServerProcess()
        {
            eventStoreProcess = EventStoreServerStarterHelper.StartServer();
        }

        [AfterTestRun]
        public static void TeardownNode()
        {
            EventStoreServerStarterHelper.StopProcess(eventStoreProcess);
        }

        public async Task<IManageSessionOf<IHoldHigherOrder>> StartSession(Guid currentStreamId)
        {
            var session = await repository.BeginSessionFor(currentStreamId.ToString()).ConfigureAwait(false);
            return session;

        }

        public async Task AppendEventsToCurrentStream(Guid id, MyEvent[] events)
        {
            using (var session = await StartSession(id))
            {
                session.AddEvents(events);
                await session.SaveChanges();
            }
        }

        public async Task SoftDeleteStream(Guid id)
            => await repository.Delete(id.ToString());

        public async Task HardDeleteStream(Guid id)
            => await GetConnection().DeleteStreamAsync(id.ToString(), -1, true);

        public async Task<ResolvedEvent[]> ReadEventsFromStreamRaw(Guid id)
        {
            var conn = GetConnection();
            var result = new List<ResolvedEvent>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = await conn.ReadStreamEventsForwardAsync(id.ToString(), nextSliceStart, 100, false);
                nextSliceStart = currentSlice.NextEventNumber;
                result.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            return result.ToArray();
        }

        internal Task WriteEventsToStreamRaw(Guid currentStreamInUse, IEnumerable<MyEvent> myEvents)
        {
            var conn = GetConnection();
            return conn.AppendToStreamAsync(currentStreamInUse.ToString(), ExpectedVersion.Any,
                myEvents.Select(e =>
                {
                    var serialized = JsonConvert.SerializeObject(e);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(serialized);
                    return new EventData(Guid.NewGuid(),
                        e.GetType().AssemblyQualifiedName,
                        true,
                        bytes,
                        null);
                }));
        }
    }
}
