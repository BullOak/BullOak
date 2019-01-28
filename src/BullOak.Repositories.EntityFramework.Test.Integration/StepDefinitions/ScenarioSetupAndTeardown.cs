namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using BullOak.Repositories.EntityFramework.Test.Integration.Contexts;
    using BullOak.Repositories.EntityFramework.Test.Integration.DbModel;
    using TechTalk.SpecFlow;

    [Binding]
    internal class ScenarioSetupAndTeardown
    {
        private ClientIdContainer clientIdContainer;
        private ConfigurationContainer configurationContainer;
        private EntityFrameworkRepositoryContainer repoContainer;
        private TestContextContainer testContextContainer;

        public ScenarioSetupAndTeardown(ClientIdContainer clientIdContainer,
            TestContextContainer testContextContainer,
            ConfigurationContainer configurationContainer,
            EntityFrameworkRepositoryContainer repoContainer)
        {
            this.clientIdContainer = clientIdContainer;
            this.repoContainer = repoContainer;
            this.configurationContainer = configurationContainer;
            this.testContextContainer = testContextContainer;
        }

        [BeforeTestRun]
        public static void TestSetup()
        {
            var config = ConfigurationContainer.GetNewConfiguration();

            using (var ctx = new TestContext())
            {
                foreach (var order in ctx.Orders)
                    ctx.Orders.Remove(order);

                ctx.SaveChanges();
            }
        }

        [BeforeScenario]
        public void Setup()
        {
            clientIdContainer.ResetToNew();
            configurationContainer.Setup();
            repoContainer.Setup(configurationContainer.Configuration);
        }
    }
}
