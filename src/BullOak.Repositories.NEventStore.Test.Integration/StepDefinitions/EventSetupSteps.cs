namespace BullOak.Repositories.NEventStore.Test.Integration.StepDefinitions
{
    using System.Linq;
    using BullOak.Repositories.NEventStore.Test.Integration.Contexts;
    using FluentAssertions;
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

        [Given(@"(.*) new events? starting with Order of (.*)")]
        public void GivenNewEvent(int count, int order)
            => eventsContainer.LastEventsCreated = generator.GenerateEvents(count).Select(x =>
                new MyEvent
                {
                    Order = x.Order + order,
                    Id = x.Id,
                }).ToArray();
    }
}
