namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;
    using TechTalk.SpecFlow;

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
