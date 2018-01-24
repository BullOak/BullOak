﻿namespace BullOak.Repositories.Test.Acceptance.StepDefinitions
{
    using BullOak.Repositories.Test.Acceptance.Contexts;
    using TechTalk.SpecFlow;

    [Binding]
    internal class EventSetupSteps
    {
        private EventGenerator generator;
        private NewEventsContainer eventsContainer;

        public EventSetupSteps(EventGenerator generator, NewEventsContainer eventsContainer)
        {
            this.generator = generator;
            this.eventsContainer = eventsContainer;
        }

        [Given(@"(.*) new events?")]
        public void GivenNewEvent(int count)
            => eventsContainer.LastEventsCreated = generator.GenerateEvents(count);
    }
}
