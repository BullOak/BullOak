namespace BullOak.Repositories.EventStore.Test.Integration.StepDefinitions
{
    using BullOak.Repositories.EventStore.Test.Integration.Components;
    using BullOak.Repositories.EventStore.Test.Integration.Contexts;
    using BullOak.Repositories.Exceptions;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TechTalk.SpecFlow;
    using Xunit;

    [Binding]
    internal class SaveEventsStreamSteps
    {
        private readonly InProcEventStoreIntegrationContext eventStoreContainer;
        private readonly EventGenerator eventGenerator;
        private readonly TestDataContext testDataContext;

        public SaveEventsStreamSteps(
            InProcEventStoreIntegrationContext eventStoreContainer,
            EventGenerator eventGenerator,
            TestDataContext testDataContext)
        {
            this.eventStoreContainer = eventStoreContainer ?? throw new ArgumentNullException(nameof(eventStoreContainer));
            this.eventGenerator = eventGenerator ?? throw new ArgumentNullException(nameof(eventGenerator));
            this.testDataContext = testDataContext ?? throw new ArgumentNullException(nameof(testDataContext));
        }

        [Given(@"a new stream")]
        public void GivenANewStream()
        {
            testDataContext.ResetStream();
        }

        [Given(@"an existing stream with (.*) events")]
        public Task GivenAnExistingStreamWithEvents(int count)
        {
            testDataContext.ResetStream();
            return eventStoreContainer.WriteEventsToStreamRaw(
                testDataContext.CurrentStreamId,
                eventGenerator.GenerateEvents(count));
        }

        [Given(@"(.*) new events?")]
        public void GivenNewEvents(int eventsNumber)
        {
            var events = eventGenerator.GenerateEvents(eventsNumber);
            testDataContext.LastGeneratedEvents = events;
        }

        [Given(@"I try to save the new events in the stream through their interface")]
        [When(@"I try to save the new events in the stream through their interface")]
        public async Task GivenITryToSaveTheNewEventsInTheStreamThroughTheirInterface()
        {
            testDataContext.RecordedException = await Record.ExceptionAsync(async () => 
            {
                using (var session = await eventStoreContainer.StartSession(testDataContext.CurrentStreamId))
                {
                    foreach (var @event in testDataContext.LastGeneratedEvents)
                    {
                        session.AddEvent<IMyEvent>(m =>
                        {
                            m.Id = @event.Id;
                            m.Value = @event.Value;
                        });
                    }

                    await session.SaveChanges();
                }
            });
        }

        [When(@"I try to save the new events in the stream")]
        public async Task WhenITryToSaveTheNewEventsInTheStream()
        {
            testDataContext.RecordedException = await Record.ExceptionAsync(() =>
                eventStoreContainer.AppendEventsToCurrentStream(
                    testDataContext.CurrentStreamId,
                    testDataContext.LastGeneratedEvents));
        }

        [Given(@"I soft-delete the stream")]
        public async Task GivenISoft_DeleteTheStream()
            => await eventStoreContainer.SoftDeleteStream(testDataContext.CurrentStreamId);

        [Given(@"I hard-delete the stream")]
        public async Task GivenIHard_DeleteTheStream()
            => await eventStoreContainer.HardDeleteStream(testDataContext.CurrentStreamId);

        [Then(@"the load process should succeed")]
        [Then(@"the save process should succeed")]
        public void ThenTheSaveProcessShouldSucceed()
        {
            testDataContext.RecordedException.Should().BeNull();
        }

        [Then(@"the save process should fail")]
        public void ThenTheSaveProcessShouldFail()
        {
            testDataContext.RecordedException.Should().NotBeNull();
        }

        [Then(@"there should be (.*) events in the stream")]
        public async Task ThenThereShouldBeEventsInTheStream(int count)
        {
            var recordedEvents = await eventStoreContainer.ReadEventsFromStreamRaw(testDataContext.CurrentStreamId);
            recordedEvents.Length.Should().Be(count);
        }

        [When(@"I load my entity")]
        public async Task WhenILoadMyEntity()
        {
            if (testDataContext.RecordedException != null) return;

            testDataContext.RecordedException = await Record.ExceptionAsync(async () =>
            {
                using (var session = await eventStoreContainer.StartSession(testDataContext.CurrentStreamId))
                {
                    testDataContext.LatestLoadedState = session.GetCurrentState();
                }
            });
        }

        [Then(@"HighOrder property should be (.*)")]
        public void ThenHighOrderPropertyShouldBe(int highestOrderValue)
        {
            testDataContext.LatestLoadedState.HigherOrder.Should().Be(highestOrderValue);
        }

        [When(@"I add (.*) events in the session without saving it")]
        public async Task WhenIAddEventsInTheSessionWithoutSavingIt(int eventCount)
        {
            using (var session = await eventStoreContainer.StartSession(testDataContext.CurrentStreamId))
            {
                session.AddEvents(eventGenerator.GenerateEvents(eventCount));

                testDataContext.LatestLoadedState = session.GetCurrentState();
            }
        }

        [Given(@"session '(.*)' is open")]
        public async Task GivenSessionIsOpen(string sessionName)
        {
            testDataContext.NamedSessions.Add(
                sessionName,
                await eventStoreContainer.StartSession(testDataContext.CurrentStreamId));
        }

        [When(@"I try to add (.*) new events to '(.*)'")]
        [Given(@"(.*) new events are added by '(.*)'")]
        public void GivenNewEventsAreAddedBy(int count, string sessionName)
        {
            testDataContext.NamedSessions[sessionName].AddEvents(eventGenerator.GenerateEvents(count));
        }

        [When(@"I try to save '(.*)'")]
        public async Task WhenITryToSave(string sessionName)
        {
            if (!testDataContext.NamedSessionsExceptions.ContainsKey(sessionName))
            {
                testDataContext.NamedSessionsExceptions.Add(sessionName, new List<Exception>());
            }
            var recordedException = await Record.ExceptionAsync(() => testDataContext.NamedSessions[sessionName].SaveChanges());
            if (recordedException != null)
            {
                testDataContext.NamedSessionsExceptions[sessionName].Add(recordedException);
            }
        }

        [Then(@"the save process should succeed for '(.*)'")]
        public void ThenTheSaveProcessShouldSucceedFor(string sessionName)
        {
            testDataContext.NamedSessionsExceptions[sessionName].Should().BeEmpty();
        }

        [Then(@"the save process should fail for '(.*)' with ConcurrencyException")]
        public void ThenTheSaveProcessShouldFailForWithConcurrencyException(string sessionName)
        {
            testDataContext.NamedSessionsExceptions[sessionName].Should().NotBeEmpty();
            testDataContext.NamedSessionsExceptions[sessionName].Count.Should().Be(1);
            testDataContext.NamedSessionsExceptions[sessionName][0].Should().BeOfType<ConcurrencyException>();
        }

        [Then(@"the save process should fail for '(.*)'")]
        public void ThenTheSaveProcessShouldFailFor(string sessionName)
        {
            testDataContext.NamedSessionsExceptions[sessionName].Should().NotBeEmpty();
        }
    }
}
