namespace BullOak.Repositories.EntityFramework.Test.Integration.Contexts
{
    using System;
    using System.Linq;
    using TechTalk.SpecFlow;

    internal class EventGenerator
    {
        public MyEvent[] GenerateEvents(int count)
            => Enumerable.Range(0, count).Select(x => new MyEvent(x)).ToArray();
    }

    internal class NewEventsContainer
    {
        private static readonly string eventsKey = Guid.NewGuid().ToString();
        private ScenarioContext ScenarioContext { get; }

        public NewEventsContainer(ScenarioContext scenarioContext)
        {
            ScenarioContext = scenarioContext;
        }

        public MyEvent[] LastEventsCreated
        {
            get => (MyEvent[]) ScenarioContext[eventsKey];
            set => ScenarioContext[eventsKey] = value;
        }
    }
}
