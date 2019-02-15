[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace BullOak.Repositories.EventStore.Test.Integration.StepDefinitions
{
    using BoDi;
    using BullOak.Repositories.EventStore.Test.Integration.Contexts;
    using System;
    using System.Threading.Tasks;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class TestsSetupAndTeardown
    {
        private readonly IObjectContainer objectContainer;

        public TestsSetupAndTeardown(IObjectContainer objectContainer)
        {
            this.objectContainer = objectContainer ?? throw new ArgumentNullException(nameof(objectContainer));
        }

        [BeforeTestRun]
        public static Task SetupEventStoreNode()
        {
            return InProcEventStoreIntegrationContext.SetupNode();
        }

        [AfterTestRun]
        public static void TeardownNode()
        {
            InProcEventStoreIntegrationContext.TeardownNode();
        }
    }
}
