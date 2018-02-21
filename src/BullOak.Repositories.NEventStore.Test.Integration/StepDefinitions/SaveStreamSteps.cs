namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using System;
    using System.Threading.Tasks;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using FluentAssertions;
    using TechTalk.SpecFlow;
    using Xunit;

    [Binding]
    internal class SaveStreamSteps
    {
        private EventGenerator eventGenerator;
        private NewEventsContainer eventsContainer;
        private StreamInfoContainer streamInfo;
        private NEventStoreSessionContainer sessionContainer;
        private LastAccessedStateContainer lastStateContainer;

        private Exception recordedException;

        public SaveStreamSteps(NewEventsContainer eventsContainer,
            EventGenerator eventGenerator,
            StreamInfoContainer streamInfoContainer,
            LastAccessedStateContainer lastStateContainer,
            NEventStoreSessionContainer sessionContainer)
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
            using (var session = await sessionContainer.StartSession(streamInfo.Id))
            {
                session.AddEvents(eventsContainer.LastEventsCreated);

                //The implementation is sync-based. Async it stubbed.
                recordedException = await Record.ExceptionAsync(() => session.SaveChanges());
            }
        }

        [Given(@"I try to save the new events in the stream through their interface")]
        [When(@"I try to save the new events in the stream through their interface")]
        public async Task WhenITryToSaveTheNewEventsInTheStreamThroughTheirInterface()
        {
            using (var session = await sessionContainer.StartSession(streamInfo.Id))
            {
                foreach (var e in eventsContainer.LastEventsCreated)
                {
                    session.AddEvent<IMyEvent>(x =>
                    {
                        x.Id = e.Id;
                        x.Order = e.Order;
                    });
                }

                //The implementation is sync-based. Async it stubbed.
                recordedException = await Record.ExceptionAsync(() => session.SaveChanges());
            }
        }


        [When(@"I add (.*) events? in the session without saving it")]
        public async Task WhenIAddEventsInTheSession(int eventCount)
        {
            using (var session = await sessionContainer.StartSession(streamInfo.Id))
            {
                session.AddEvents(eventGenerator.GenerateEvents(eventCount));
                
                lastStateContainer.LatestLoadedState = session.GetCurrentState();
            }
        }

        [Then(@"the save process should succeed")]
        public void ThenTheSaveProcessShouldSucceed()
        {
            recordedException.Should().BeNull();
        }
    }
}
