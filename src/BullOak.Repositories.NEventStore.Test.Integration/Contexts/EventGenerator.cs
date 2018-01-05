namespace BullOak.Repositories.NEventStore.Test.Integration.Contexts
{
    using System;
    using System.Linq;
    using BullOak.Repositories.Appliers;
    using TechTalk.SpecFlow;

    public class MyEvent
    {
        public Guid Id { get; private set; }
        public int Order { get; private set; }

        public MyEvent(int order)
        {
            Id = Guid.NewGuid();
            Order = order;
        }
    }

    public interface IHoldHigherOrder
    {
        int HigherOrder { get; set; }
    }

    public class StateApplier : IApplyEvent<IHoldHigherOrder, MyEvent>
    {
        public IHoldHigherOrder Apply(IHoldHigherOrder state, MyEvent @event)
        {
            state.HigherOrder = Math.Max(@event.Order, state.HigherOrder);

            return state;
        }
    }

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
