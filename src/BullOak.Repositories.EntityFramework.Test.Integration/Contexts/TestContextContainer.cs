namespace BullOak.Repositories.EntityFramework.Test.Integration.Contexts
{
    using System;
    using BullOak.Repositories.EntityFramework.Test.Integration.DbModel;
    using TechTalk.SpecFlow;

    internal class TestContextContainer : IDisposable
    {
        private static readonly string storeId = Guid.NewGuid().ToString();

        private ScenarioContext scenarioContext;
        public TestContext TestContext
        {
            get => (TestContext) scenarioContext[storeId];
            private set => scenarioContext[storeId] = value;
        }

        public TestContextContainer(ScenarioContext scenarioContext)
            => this.scenarioContext = scenarioContext;

        public void Setup()
        {

        }

        public TestContext StartContext()
        {
            TestContext = new TestContext();
            return TestContext;
        }

        public void Dispose()
        {
            TestContext?.Dispose();
        }
    }
}
