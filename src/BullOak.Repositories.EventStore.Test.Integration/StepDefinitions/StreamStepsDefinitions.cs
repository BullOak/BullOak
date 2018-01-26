
namespace BullOak.Repositories.EventStore.Test.Integration.StepDefinitions
{
    using BullOak.Repositories.EventStore.Test.Integration.Components;
    using BullOak.Repositories.EventStore.Test.Integration.Contexts;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using TechTalk.SpecFlow;
    using Xunit;

    [Binding]
    internal class SaveEventsStreamSteps
    {
        private readonly InProcEventStoreIntegrationContext eventStoreContainer;
        private readonly EventGenerator eventGenerator;
        private readonly TestDataContext testDataContext;


        public SaveEventsStreamSteps(InProcEventStoreIntegrationContext eventStoreContainer,
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
        public void GivenAnExistingStreamWithEvents(int count)
        {
            testDataContext.ResetStream();
            eventStoreContainer.WriteEventsToStreamRaw(
                testDataContext.CurrentStreamId,
                eventGenerator.GenerateEvents(count));
        }

        [Given(@"(.*) new events")]
        public void GivenNewEvents(int eventsNumber)
        {
            var events = eventGenerator.GenerateEvents(eventsNumber);
            testDataContext.LastGeneratedEvents = events;
        }

        [When(@"I try to save the new events in the stream")]
        public void WhenITryToSaveTheNewEventsInTheStream()
        {
            testDataContext.RecordedException = Record.Exception(() =>
                eventStoreContainer.AppendEventsToCurrentStream(testDataContext.CurrentStreamId, testDataContext.LastGeneratedEvents).Wait());
        }

        [Then(@"the save process should succeed")]
        public void ThenTheSaveProcessShouldSucceed()
        {
            testDataContext.RecordedException.Should().BeNull();
        }

        [Then(@"there should be (.*) events in the stream")]
        public void ThenThereShouldBeEventsInTheStream(int count)
        {
            var recordedEvents = eventStoreContainer.ReadEventsFromStreamRaw(testDataContext.CurrentStreamId);
            recordedEvents.Length.Should().Be(count);
        }

        [When(@"I load my entity")]
        public void WhenILoadMyEntity()
        {
            using (var session = eventStoreContainer.StartSession(testDataContext.CurrentStreamId).Result)
            {
                testDataContext.LatestLoadedState = session.GetCurrentState();
            }
        }

        [Then(@"HighOrder property should be (.*)")]
        public void ThenHighOrderPropertyShouldBe(int highestOrderValue)
        {
            testDataContext.LatestLoadedState.HigherOrder.Should().Be(highestOrderValue);
        }

        [When(@"I add (.*) events in the session without saving it")]
        public void WhenIAddEventsInTheSessionWithoutSavingIt(int eventCount)
        {
            using (var session = eventStoreContainer.StartSession(testDataContext.CurrentStreamId).Result)
            {
                session.AddEvents(eventGenerator.GenerateEvents(eventCount));

                testDataContext.LatestLoadedState = session.GetCurrentState();
            }
        }

        [Given(@"session '(.*)' is open")]
        public void GivenSessionIsOpen(string sessionName)
        {
            testDataContext.NamedSessions.Add(sessionName, eventStoreContainer.StartSession(testDataContext.CurrentStreamId).Result);
        }

        [When(@"I try to add (.*) new events to '(.*)'")]
        [Given(@"(.*) new events are added by '(.*)'")]
        public void GivenNewEventsAreAddedBy(int count, string sessionName)
        {
            testDataContext.NamedSessions[sessionName].AddEvents(eventGenerator.GenerateEvents(count));
        }

        [When(@"I try to save '(.*)'")]
        public void WhenITryToSave(string sessionName)
        {
            if (!testDataContext.NamedSessionsExceptions.ContainsKey(sessionName))
            {
                testDataContext.NamedSessionsExceptions.Add(sessionName, new List<Exception>());
            }
            var recordedException = Record.ExceptionAsync(() => testDataContext.NamedSessions[sessionName].SaveChanges()).Result;
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

        [Then(@"the save process should fail for '(.*)'")]
        public void ThenTheSaveProcessShouldFailFor(string sessionName)
        {
            testDataContext.NamedSessionsExceptions[sessionName].Should().NotBeEmpty();
        }

    }
}
