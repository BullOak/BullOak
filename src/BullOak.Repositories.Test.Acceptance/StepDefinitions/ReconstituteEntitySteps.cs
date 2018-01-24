namespace BullOak.Repositories.Test.Acceptance.StepDefinitions
{
    using System;
    using BullOak.Repositories.Test.Acceptance.Contexts;
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

        [Then(@"FullName property of the entity should be ""(.*)""")]
        public void ThenFullNamePropertyOfTheEntityShouldBe(string fullName)
        {
            lastStateContainer.LatestLoadedState.FullName.Should().Be(fullName);
        }

        [Then(@"the loaded entity should have a balance of (.*) and last update date (.*)")]
        public void ThenTheLoadedEntityShouldHaveABalanceOfAndLastUpdateDate(Decimal balance, DateTime dateTime)
        {
            lastStateContainer.LatestLoadedState.LastBalance.Should().Be(balance);
            lastStateContainer.LatestLoadedState.BalaceUpdateTime.Should().Be(dateTime);
        }
    }
}
