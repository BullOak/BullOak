namespace BullOak.Repositories.EntityFramework.Test.Integration.Contexts
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.EntityFramework.Test.Integration.DbModel;
    using BullOak.Repositories.Session;
    using TechTalk.SpecFlow;

    internal class EntityFrameworkRepositoryContainer : IDisposable
    {
        private static readonly string id = Guid.NewGuid().ToString();
        private static readonly string repoId = Guid.NewGuid().ToString();

        private ScenarioContext scenarioContext;
        private TestContextContainer testContextContainer;

        public IManageSessionOf<HoldHighOrders> LastSession
        {
            get => (IManageSessionOf<HoldHighOrders>) scenarioContext[id];
            private set => scenarioContext[id] = value;
        }

        public EntityFrameworkRepository<TestContext, HoldHighOrders> Repo
        {
            get
            {
                if (scenarioContext.ContainsKey(repoId))
                    return (EntityFrameworkRepository<TestContext, HoldHighOrders>) scenarioContext[repoId];
                return default(EntityFrameworkRepository<TestContext, HoldHighOrders>);
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
            Repo = new EntityFrameworkRepository<TestContext, HoldHighOrders>(configuration, () => testContextContainer.TestContext);
        }

        public async Task<IManageSessionOf<HoldHighOrders>> StartSession(string clientId)
        {
            LastSession = await Repo.BeginSessionFor(o => o.ClientId == clientId);

            return LastSession;
        }

        public void Dispose()
        {
            if(scenarioContext.ContainsKey(id))
                LastSession?.Dispose();
        }
    }
}