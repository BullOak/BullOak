namespace BullOak.Repositories.NEventStore.Test.Integration.Contexts
{
    using System;
    using BullOak.Repositories.InMemory;
    using BullOak.Repositories.Session;
    using TechTalk.SpecFlow;

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

    internal class InMemoryStoreSessionContainer : IDisposable
    {
        private static readonly string id = Guid.NewGuid().ToString();

        private ScenarioContext scenarioContext;
        private InMemoryEventSourcedRepository<string, IHoldHigherOrder> repository;

        public IManageAndSaveSession<IHoldHigherOrder> LastSession
        {
            get => (IManageAndSaveSession<IHoldHigherOrder>) scenarioContext[id];
            private set => scenarioContext[id] = value;
        }

        public InMemoryStoreSessionContainer(StreamInfoContainer streamInfo, ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        public void Setup(IHoldAllConfiguration configuration)
        {
            if (repository != null) throw new Exception($"{nameof(repository)} already setup");

            repository = new InMemoryEventSourcedRepository<string, IHoldHigherOrder>(configuration);
        }

        public IManageAndSaveSession<IHoldHigherOrder> StartSession(string streamId)
        {
            LastSession = repository.BeginSessionFor(streamId);

            return LastSession;
        }

        public object[] GetStream(string id)
            => repository[id];

        public void SaveStream(string id, object[] events)
            => repository[id] = events;

        public void Dispose()
        {
            LastSession?.Dispose();
        }
    }
}
