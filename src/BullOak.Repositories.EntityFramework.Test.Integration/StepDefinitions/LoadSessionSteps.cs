namespace BullOak.Repositories.EntityFramework.Test.Integration.StepDefinitions
{
    using BullOak.Repositories.EntityFramework.Test.Integration.Contexts;
    using FluentAssertions;
    using TechTalk.SpecFlow;

    [Binding]
    internal class LoadSessionSteps
    {
        private readonly ClientIdContainer clientIdContainer;
        private readonly EntityFrameworkRepositoryContainer entityFrameworkRepositoryContainer;

        public LoadSessionSteps(ClientIdContainer clientIdContainer,
            EntityFrameworkRepositoryContainer entityFrameworkRepositoryContainer)
        {
            this.clientIdContainer = clientIdContainer;
            this.entityFrameworkRepositoryContainer = entityFrameworkRepositoryContainer;
        }

        [When(@"I load the session")]
        public void WhenILoadTheSession()
            => entityFrameworkRepositoryContainer.StartSession(clientIdContainer.Id);

        [Then(@"the IsNewState should be (.*)")]
        public void ThenTheIsNewStateShouldBeTrue(bool expectedSessionIsNewValue)
            => entityFrameworkRepositoryContainer.LastSession.IsNewState.Should().Be(expectedSessionIsNewValue);
    }
}
