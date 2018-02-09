namespace BullOak.Repositories.EntityFramework.Test.Integration.StepDefinitions
{
    using System;
    using System.Threading.Tasks;
    using BullOak.Repositories.EntityFramework.Test.Integration.Contexts;
    using BullOak.Repositories.EntityFramework.Test.Integration.DbModel;
    using FluentAssertions;
    using TechTalk.SpecFlow;

    [Binding]
    internal class ReconstituteEntitySteps
    {
        private ClientIdContainer clientIdContainer;
        private EntityFrameworkRepositoryContainer repoContainer;
        private LastAccessedStateContainer lastStateContainer;

        public ReconstituteEntitySteps(ClientIdContainer clientIdContainer,
            LastAccessedStateContainer lastStateContainer,
            EntityFrameworkRepositoryContainer repoContainer)
        {
            this.lastStateContainer = lastStateContainer;
            this.clientIdContainer = clientIdContainer;
            this.repoContainer = repoContainer;
        }

        [When(@"I load my entity")]
        public async Task WhenILoadMyEntity()
        {
            using (var session = await repoContainer.StartSession(clientIdContainer.Id))
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
        public IHoldHighOrders LatestLoadedState
        {
            get => (IHoldHighOrders) scenarioContext[id];
            set => scenarioContext[id] = value;
        }

        public LastAccessedStateContainer(ScenarioContext scenarioContext)
            => this.scenarioContext = scenarioContext;
    }
}
