namespace BullOak.Repositories.NEventStore.Test.Integration.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using BullOak.Repositories.Session;
    using TechTalk.SpecFlow;
    using global::NEventStore;
    using System.Configuration;
    using global::NEventStore.Persistence.Sql.SqlDialects;
    using global::NEventStore.Serialization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using BullOak.Repositories.StateEmit;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class StreamInfoContainer
    {
        private static readonly string IdKey = Guid.NewGuid().ToString();
        private ScenarioContext ScenarioContext;

        public string Id
        {
            get => (String) ScenarioContext[IdKey];
            set => ScenarioContext[IdKey] = value;
        }

        public string Stream2Id
        {
            get => (String) ScenarioContext[IdKey + nameof(Stream2Id)];
            set => ScenarioContext[IdKey + nameof(Stream2Id)] = value;
        }

        public string Stream3Id
        {
            get => (String) ScenarioContext[IdKey + nameof(Stream3Id)];
            set => ScenarioContext[IdKey + nameof(Stream3Id)] = value;
        }

        public int Revision
        {
            get => (int) ScenarioContext[IdKey + nameof(Revision)];
            set => ScenarioContext[IdKey + nameof(Revision)] = value;
        }

        public StreamInfoContainer(ScenarioContext scenarioContext)
            => ScenarioContext = scenarioContext;

        public void ResetToNew()
        {
            Id = "StreamId_" + Guid.NewGuid().ToString();
            Stream2Id = "StreamId_" + Guid.NewGuid().ToString();
            Stream3Id = "StreamId_" + Guid.NewGuid().ToString();
            Revision = 0;
        }
    }

    internal class NEventStoreContainer : IDisposable
    {
        private static readonly string storeId = Guid.NewGuid().ToString();

        private ScenarioContext scenarioContext;
        public IStoreEvents EventStore
        {
            get => (IStoreEvents) scenarioContext[storeId];
            private set => scenarioContext[storeId] = value;
        }

        public NEventStoreContainer(ScenarioContext scenarioContext)
            => this.scenarioContext = scenarioContext;

        public void Setup(IHoldAllConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("BullOak_NEventStore_Sql", EnvironmentVariableTarget.User);

            EventStore = Wireup.Init()
                .UsingSqlPersistence("bo_db", "System.Data.SqlClient", connectionString)
                .WithDialect(new MsSqlDialect())
                .InitializeStorageEngine()
                .WithSerializationForInterfaceMessages(configuration)
                .Build();

            EventStore.Advanced.Initialize();
            EventStore.Advanced.Purge();
        }


        public IEventStream OpenStream(string id, int minRevision)
            => EventStore.OpenStream(id, minRevision);

        public void Dispose()
        {
            EventStore?.Dispose();
        }
    }

    internal class NEventStoreSessionContainer : IDisposable
    {
        private static readonly string id = Guid.NewGuid().ToString();

        private ScenarioContext scenarioContext;
        private NEventStoreContainer nEventStoreContainer;
        private NEventStoreRepository<string, IHoldHigherOrder> repository;

        public IManageSessionOf<IHoldHigherOrder> LastSession
        {
            get => (IManageSessionOf<IHoldHigherOrder>) scenarioContext[id];
            private set => scenarioContext[id] = value;
        }

        public NEventStoreSessionContainer(NEventStoreContainer neventStoreContainer, StreamInfoContainer streamInfo, ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
            this.nEventStoreContainer = neventStoreContainer;
        }

        public void Setup(IHoldAllConfiguration configuration)
        {
            if (repository != null) throw new Exception($"{nameof(repository)} already setup");

            repository = new NEventStoreRepository<string, IHoldHigherOrder>(nEventStoreContainer.EventStore, configuration);
        }

        public async Task<IManageSessionOf<IHoldHigherOrder>> StartSession(string streamId)
        {
            LastSession = await repository.BeginSessionFor(streamId);

            return LastSession;
        }

        public void Dispose()
        {
            LastSession?.Dispose();
        }
    }
}
