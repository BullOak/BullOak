namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using System.Reflection;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using TechTalk.SpecFlow;

    [Binding]
    internal class ScenarioSetupAndTeardown
    {
        private readonly StreamInfoContainer streamInfoContainer;
        private readonly NEventStoreContainer neventStoreContainer;
        private readonly NEventStoreSessionContainer sessionContainer;

        public ScenarioSetupAndTeardown(StreamInfoContainer streamInfoContainer, NEventStoreContainer neventStoreContainer,
            NEventStoreSessionContainer sessionContainer)
        {
            this.streamInfoContainer = streamInfoContainer;
            this.neventStoreContainer = neventStoreContainer;
            this.sessionContainer = sessionContainer;
        }

        [BeforeScenario]
        public void Setup()
        {
            streamInfoContainer.ResetToNew();
            neventStoreContainer.Setup();
            
            var configuration = Configuration.Begin()
                .WithDefaultCollection()
                .WithDefaultStateFactory()
                .NeverUseThreadSafe()
                .WithNoEventPublisher()
                .WithAnyAppliersFrom(Assembly.GetExecutingAssembly())
                .Build();

            sessionContainer.Setup(configuration);
        }
    }
}
