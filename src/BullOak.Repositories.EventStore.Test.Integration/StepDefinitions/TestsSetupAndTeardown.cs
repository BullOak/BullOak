
namespace BullOak.Repositories.EventStore.Test.Integration.StepDefinitions
{
    using BoDi;
    using BullOak.Repositories.EventStore.Test.Integration.Components;
    using BullOak.Repositories.EventStore.Test.Integration.Contexts;
    using System;
    using BullOak.Repositories.Config;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class TestsSetupAndTeardown
    {
        private readonly IObjectContainer objectContainer;

        public TestsSetupAndTeardown(IObjectContainer objectContainer)
        {
            this.objectContainer = objectContainer ?? throw new ArgumentNullException(nameof(objectContainer));
        }


        [BeforeScenario]
        public void SetupEventStore()
        {
            var configuration = Configuration.Begin()
               .WithDefaultCollection()
               .WithDefaultStateFactory()
               .NeverUseThreadSafe()
               .WithNoEventPublisher()
               //.WithAnyAppliersFrom(Assembly.GetExecutingAssembly())
               .WithEventApplier(new StateApplier())
               .AndNoMoreAppliers()
               .WithNoUpconverters()
               .Build();

            var storeContainer = new InProcEventStoreIntegrationContext();
            storeContainer.Setup(configuration);

            objectContainer.RegisterInstanceAs(storeContainer);

        }


        [AfterScenario]
        public void Teardown()
        {
            var context = objectContainer.Resolve<InProcEventStoreIntegrationContext>();
            context.Teardown();
        }

    }
}
