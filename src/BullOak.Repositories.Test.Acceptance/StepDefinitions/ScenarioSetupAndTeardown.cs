namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using System.Reflection;
    using BullOak.Repositories.Config;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using TechTalk.SpecFlow;

    [Binding]
    internal class ScenarioSetupAndTeardown
    {
        private readonly StreamInfoContainer streamInfoContainer;
        private readonly InMemoryStoreSessionContainer sessionContainer;

        public ScenarioSetupAndTeardown(StreamInfoContainer streamInfoContainer,
            InMemoryStoreSessionContainer sessionContainer)
        {
            this.streamInfoContainer = streamInfoContainer;
            this.sessionContainer = sessionContainer;
        }

        [BeforeScenario]
        public void Setup()
        {
            streamInfoContainer.ResetToNew();
            
            var configuration = Configuration.Begin()
                .WithDefaultCollection()
                .WithDefaultStateFactory()
                .NeverUseThreadSafe()
                .WithNoEventPublisher()
                .WithAnyAppliersFrom(Assembly.GetExecutingAssembly())
                .AndNoMoreAppliers()
                .WithNoUpconverters()
                .Build();

            sessionContainer.Setup(configuration);
        }
    }
}
