namespace BullOak.Repositories.Test.Acceptance.StepDefinitions
{
    using BullOak.Repositories.Test.Acceptance.Contexts;
    using FluentAssertions;
    using TechTalk.SpecFlow;

    [Binding]
    internal class LoadStreamSteps
    {
        public InMemoryStoreSessionContainer SessionContainer { get; }
        public StreamInfoContainer StreamInfoContainer { get; }

        public LoadStreamSteps(InMemoryStoreSessionContainer sessionContainer, StreamInfoContainer streamInfoContainer)
        {
            this.SessionContainer = sessionContainer;
            this.StreamInfoContainer = streamInfoContainer;
        }

        [When(@"I load the session")]
        public void WhenILoadTheState()
            => SessionContainer.StartSession(StreamInfoContainer.Id);

        [Then(@"the IsNewState should be (.*)")]
        public void ThenTheIsNewStateShouldBe(bool expectedStateIsNewValue)
            => SessionContainer.LastSession.IsNewState.Should().Be(expectedStateIsNewValue);
    }
}
