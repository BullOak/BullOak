namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using System;
    using System.Net.Mail;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using FluentAssertions;
    using TechTalk.SpecFlow;

    [Binding]
    internal class ReconstituteEntitySteps
    {
        private StreamInfoContainer streamInfo;
        private InMemoryStoreSessionContainer sessionContainer;
        private LastAccessedStateContainer lastStateContainer;

        public ReconstituteEntitySteps(StreamInfoContainer streamInfoContainer,
            LastAccessedStateContainer lastStateContainer,
            InMemoryStoreSessionContainer sessionContainer)
        {
            this.lastStateContainer = lastStateContainer;
            this.streamInfo = streamInfoContainer;
            this.sessionContainer = sessionContainer;
        }

        [When(@"I load my entity")]
        public void WhenILoadMyEntity()
        {
            using (var session = sessionContainer.StartSession(streamInfo.Id))
            {
                lastStateContainer.LatestLoadedState = session.GetCurrentState();
            }
        }

        [Then(@"HighOrder property should be (.*)")]
        public void ThenHighOrderPropertyShouldBe(int expectedHighOrder)
        {
            lastStateContainer.LatestLoadedState.HigherOrder.Should().Be(expectedHighOrder);
        }
    }

    public class LastAccessedStateContainer
    {
        private static readonly string id = Guid.NewGuid().ToString();
        private ScenarioContext scenarioContext;
        public IHoldHigherOrder LatestLoadedState
        {
            get => (IHoldHigherOrder) scenarioContext[id];
            set => scenarioContext[id] = value;
        }

        public LastAccessedStateContainer(ScenarioContext scenarioContext)
            => this.scenarioContext = scenarioContext;
    }
}
