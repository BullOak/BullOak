namespace BullOak.Repositories.EntityFramework.Test.Integration.Contexts
{
    using System;
    using System.Linq;
    using BullOak.Repositories.EntityFramework.Test.Integration.DbModel;
    using BullOak.Repositories.Session;
    using TechTalk.SpecFlow;

    internal class EntityFrameworkRepositoryContainer : IDisposable
    {
        private static readonly string id = Guid.NewGuid().ToString();
        private static readonly string repoId = Guid.NewGuid().ToString();

        private ScenarioContext scenarioContext;
        private TestContextContainer testContextContainer;

        public IManageAndSaveSession<IHoldHighOrders> LastSession
        {
            get => (IManageAndSaveSession<IHoldHighOrders>) scenarioContext[id];
            private set => scenarioContext[id] = value;
        }

        public EntityFrameworkRepository<TestContext> Repo
        {
            get
            {
                if (scenarioContext.ContainsKey(repoId))
                    return (EntityFrameworkRepository<TestContext>) scenarioContext[repoId];
                return default;
            }
            private set => scenarioContext[repoId] = value;
        }

        public EntityFrameworkRepositoryContainer(TestContextContainer testContextContainer, ClientIdContainer clientIdContainer, ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
            this.testContextContainer = testContextContainer;
        }

        public void Setup(IHoldAllConfiguration configuration)
        {
            if (Repo != null) throw new Exception($"{nameof(Repo)} already setup");

            testContextContainer.StartContext();
            Repo = new EntityFrameworkRepository<TestContext>(configuration, () => testContextContainer.TestContext);
        }

        public IManageAndSaveSession<IHoldHighOrders> StartSession(string clientId)
        {
            LastSession = Repo.BeginSessionFor<IHoldHighOrders>(x => x.Orders.FirstOrDefault(o => o.ClientId == clientId));

            return LastSession;
        }

        public void Dispose()
        {
            if(scenarioContext.ContainsKey(id))
                LastSession?.Dispose();
        }
    }
}