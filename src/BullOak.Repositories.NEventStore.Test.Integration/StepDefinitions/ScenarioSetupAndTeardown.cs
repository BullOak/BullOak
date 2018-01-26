namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using BullOak.Repositories.Config;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using System.Reflection;
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
                .AndNoMoreAppliers()
                .WithNoUpconverters()
                .Build();

            sessionContainer.Setup(configuration);
        }
    }
}
