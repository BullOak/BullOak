
namespace BullOak.Repositories.EventStore.Test.Integration.StepDefinitions
{
    using BoDi;
    using BullOak.Repositories.Config;
    using BullOak.Repositories.EventStore.Test.Integration.Contexts;
    using System;
    using System.Reflection;
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
        public static void SetupEventStoreNode()
        {
            InProcEventStoreIntegrationContext.SetupNode();
        }

        [BeforeScenario]
        public void SetupEventStore()
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

            var storeContainer = new InProcEventStoreIntegrationContext();
            storeContainer.SetupRepository(configuration);

            objectContainer.RegisterInstanceAs(storeContainer);

        }

        [AfterTestRun]
        public static void TeardownNode()
        {
            InProcEventStoreIntegrationContext.TeardownNode();
        }
    }
}
