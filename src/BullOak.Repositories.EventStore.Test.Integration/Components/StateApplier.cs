namespace BullOak.Repositories.EventStore.Test.Integration.Components
{
    using BullOak.Repositories.Appliers;
    using System;

    public class StateApplier : IApplyEvent<IHoldHigherOrder, MyEvent>
    {
        public IHoldHigherOrder Apply(IHoldHigherOrder state, MyEvent @event)
        {
            state.HigherOrder = Math.Max(@event.Value, state.HigherOrder);

            return state;
        }
    }
}
