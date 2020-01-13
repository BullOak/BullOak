namespace BullOak.Repositories.Test.Acceptance.StepDefinitions
{
    using System;
    using System.ComponentModel.Design;
    using System.Threading.Tasks;
    using BullOak.Repositories.Test.Acceptance.Contexts;
    using FluentAssertions;
    using TechTalk.SpecFlow;
    using Xunit;

    [Binding]
    internal class SaveStreamSteps
    {
        private EventGenerator eventGenerator;
        private NewEventsContainer eventsContainer;
        private StreamInfoContainer streamInfo;
        private InMemoryStoreSessionContainer sessionContainer;
        private LastAccessedStateContainer lastStateContainer;

        private Exception recordedException;

        public SaveStreamSteps(NewEventsContainer eventsContainer,
            EventGenerator eventGenerator,
            StreamInfoContainer streamInfoContainer,
            LastAccessedStateContainer lastStateContainer,
            InMemoryStoreSessionContainer sessionContainer)
        {
            this.lastStateContainer = lastStateContainer;
            this.eventGenerator = eventGenerator;
            this.eventsContainer = eventsContainer;
            this.streamInfo = streamInfoContainer;
            this.sessionContainer = sessionContainer;
        }

        [When(@"I try to save the new events in the stream")]
        public async Task WhenITryToSaveTheNewEventsInTheStream()
        {
            using (var session = sessionContainer.StartSession(streamInfo.Id))
            {
                session.AddEvents(eventsContainer.LastEventsCreated);

                recordedException = await Record.ExceptionAsync(() => session.SaveChanges());
            }
        }

        [When(@"I add (.*) events? in the session without saving it")]
        public void WhenIAddEventsInTheSession(int eventCount)
        {
            using (var session = sessionContainer.StartSession(streamInfo.Id))
            {
                session.AddEvents(eventGenerator.GenerateEvents(eventCount));

                lastStateContainer.LatestLoadedState = session.GetCurrentState();
            }
        }

        [Then(@"the save process should (.*)")]
        public void ThenTheSaveProcessShould(string outcome)
        {
            if (outcome.Equals("succeed", StringComparison.CurrentCultureIgnoreCase))
            {
                recordedException.Should().BeNull();
            }
            else if (outcome.Equals("fail", StringComparison.CurrentCultureIgnoreCase))
            {
                recordedException.Should().NotBeNull();
            }
            else
            {
                throw new ArgumentException(outcome);
            }
        }
    }
}
