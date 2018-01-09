namespace BullOak.Repositories.EntityFramework.Test.Integration.Contexts
{
    using System;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EntityFramework.Test.Integration.DbModel;

    public class StateApplier : IApplyEvent<IHoldHighOrders, MyEvent>
    {
        public IHoldHighOrders Apply(IHoldHighOrders state, MyEvent @event)
        {
            state.HigherOrder = Math.Max(@event.Order, state.HigherOrder);

            return state;
        }
    }

    public class InitializeEventApplier : IApplyEvent<IHoldHighOrders, InitializeClientOrderEvent>
    {
        public IHoldHighOrders Apply(IHoldHighOrders state, InitializeClientOrderEvent @event)
        {
            state.ClientId = @event.ClientId.ToString();

            return state;
        }
    }
}