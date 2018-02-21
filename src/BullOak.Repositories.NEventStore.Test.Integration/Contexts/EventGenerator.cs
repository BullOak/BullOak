namespace BullOak.Repositories.NEventStore.Test.Integration.Contexts
{
    using System;
    using System.Linq;
    using BullOak.Repositories.Appliers;
    using TechTalk.SpecFlow;

    public interface IMyEvent
    {
        Guid Id { get; set; }
        int Order { get; set; }
    }

    public class MyEvent : IMyEvent
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
    }

    public interface IHoldHigherOrder
    {
        int HigherOrder { get; set; }
    }

    public class StateApplier : IApplyEvent<IHoldHigherOrder, MyEvent>,
        IApplyEvent<IHoldHigherOrder, IMyEvent>
    {
        IHoldHigherOrder IApplyEvent<IHoldHigherOrder, MyEvent>.Apply(IHoldHigherOrder state, MyEvent @event)
            => (this as IApplyEvent<IHoldHigherOrder, IMyEvent>).Apply(state, @event);

        public IHoldHigherOrder Apply(IHoldHigherOrder state, IMyEvent @event)
        {
            state.HigherOrder = Math.Max(@event.Order, state.HigherOrder);

            return state;
        }
    }

    internal class EventGenerator
    {
        public MyEvent[] GenerateEvents(int count)
            => Enumerable.Range(0, count).Select(x => new MyEvent
            {
                Order = x,
                Id = Guid.NewGuid()
            }).ToArray();
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
