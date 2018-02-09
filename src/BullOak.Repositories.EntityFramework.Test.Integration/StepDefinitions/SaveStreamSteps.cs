namespace BullOak.Repositories.EntityFramework.Test.Integration.StepDefinitions
{
    using System;
    using System.Threading.Tasks;
    using BullOak.Repositories.EntityFramework.Test.Integration.Contexts;
    using FluentAssertions;
    using TechTalk.SpecFlow;
    using Xunit;

    [Binding]
    internal class SaveStreamSteps
    {
        private EventGenerator eventGenerator;
        private NewEventsContainer eventsContainer;
        private ClientIdContainer clientIdContainer;
        private EntityFrameworkRepositoryContainer repositoryContainer;
        private LastAccessedStateContainer lastStateContainer;

        private Exception recordedException;

        public SaveStreamSteps(NewEventsContainer eventsContainer,
            EventGenerator eventGenerator,
            ClientIdContainer clientIdContainer,
            LastAccessedStateContainer lastStateContainer,
            EntityFrameworkRepositoryContainer repositoryContainer)
        {
            this.lastStateContainer = lastStateContainer;
            this.eventGenerator = eventGenerator;
            this.eventsContainer = eventsContainer;
            this.clientIdContainer = clientIdContainer;
            this.repositoryContainer = repositoryContainer;
        }

        [When(@"I try to save a new entity with the new events? in the stream")]
        public async Task WhenITryToSaveANewEntityWithTheNewEventsInTheStream()
        {
            using (var session = await repositoryContainer.Repo.BeginSessionFor(x=> x.ClientId == clientIdContainer.Id))
            {
                session.AddEvent(new InitializeClientOrderEvent(Guid.Parse(clientIdContainer.Id)));
                session.AddEvents(eventsContainer.LastEventsCreated);

                //The implementation is sync-based. Async it stubbed.
                recordedException = await Record.ExceptionAsync(() => session.SaveChanges());
            }
        }

        [When(@"I try to update an existing entity with the new events in the stream")]
        public async Task WhenITryToUpdateAnExistingEntityWithTheNewEventsInTheStream()
        {
            using (var session = await repositoryContainer.StartSession(clientIdContainer.Id))
            {
                session.AddEvents(eventsContainer.LastEventsCreated);

                //The implementation is sync-based. Async it stubbed.
                recordedException = await Record.ExceptionAsync(() => session.SaveChanges());
            }
        }

        [When(@"I add (.*) events? in the session of existing entity without saving it")]
        public async Task WhenIAddEventsInTheSession(int eventCount)
        {
            using (var session = await repositoryContainer.StartSession(clientIdContainer.Id))
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
