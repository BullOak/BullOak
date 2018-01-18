namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;
    using BullOak.Repositories.InMemory;
    using BullOak.Repositories.Session;
    using TechTalk.SpecFlow;

    internal class InMemoryStoreSessionContainer : IDisposable
    {
        private static readonly string id = Guid.NewGuid().ToString();

        private ScenarioContext scenarioContext;
        private InMemoryEventSourcedRepository<string, IHoldHigherOrder> repository;

        public IManageAndSaveSession<IHoldHigherOrder> LastSession
        {
            get
            {
                if(scenarioContext.ContainsKey(id))
                    return (IManageAndSaveSession<IHoldHigherOrder>) scenarioContext[id];

                return null;
            }
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