namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using BullOak.Repositories.Session;
    using FluentAssertions;
    using TechTalk.SpecFlow;

    [Binding]
    internal class SessionSteps
    {
        private EventGenerator eventGenerator;
        private StreamInfoContainer streamInfo;
        private NEventStoreSessionContainer sessionContainer;
        private NEventStoreContainer nEventStoreContainer;

        private Dictionary<int, IManageSessionOf<IHoldHigherOrder>> activeSessions = 
            new Dictionary<int, IManageSessionOf<IHoldHigherOrder>>();

        private IEnumerable<Task> saveResults;

        public SessionSteps(EventGenerator eventGenerator,
            StreamInfoContainer streamInfo,
            NEventStoreSessionContainer sessionContainer,
            NEventStoreContainer neventStoreContainer)
        {
            this.eventGenerator = eventGenerator;
            this.streamInfo = streamInfo;
            this.sessionContainer = sessionContainer;
            this.nEventStoreContainer = neventStoreContainer;
        }

        [Given(@"I start session (.*) and I add (.*) event")]
        public async Task GivenIStartSessionAndIAddEvent(int sessionIndex, int eventCount)
        {
            activeSessions[sessionIndex] = await sessionContainer.StartSession(streamInfo.Id);
            activeSessions[sessionIndex].AddEvents(eventGenerator.GenerateEvents(eventCount));
        }

        [When(@"I try to save all sessions")]
        public async Task WhenITryToSaveAllSessions()
        {
            saveResults = activeSessions.Values.Select(x => x.SaveChanges()).ToArray();

            foreach (var t in saveResults)
            {
                try
                {
                    await t;
                }
                catch
                {
                    // ignored
                }
            }
        }

        [Then(@"(.*) save session should (.*)")]
        public void ThenOneSaveSessionShould(int count, string outcome)
        {
            switch (outcome)
            {
                case "succeed":
                    saveResults.Count(x => x.Status == TaskStatus.RanToCompletion)
                        .Should().Be(count);
                    break;
                case "fail":
                    saveResults.Count(x => x.Status == TaskStatus.Faulted)
                        .Should().Be(count);
                    break;
                default:
                    throw new ArgumentException("Unexpected value");
            }
        }

        [Then(@"all failed sessions should have failed with ConcurrencyException")]
        public void ThenAllFailedSessionsShouldHaveFailedWithConcurrencyException()
        {
            var exceptions = saveResults.Where(x => x.Status == TaskStatus.Faulted)
                .SelectMany(x=> x.Exception.InnerExceptions)
                .ToArray();

            exceptions.Should().NotBeNull();
            exceptions.Should().NotContainNulls();
            exceptions.Should().AllBeOfType<ConcurrencyException>();
        }
    }
}
