namespace BullOak.Repositories.EntityFramework.Test.Integration.Contexts
{
    using System;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EntityFramework.Test.Integration.DbModel;

    public class StateApplier : IApplyEvent<HoldHighOrders, MyEvent>
    {
        public HoldHighOrders Apply(HoldHighOrders state, MyEvent @event)
        {
            state.HigherOrder = Math.Max(@event.Order, state.HigherOrder);

            return state;
        }
    }

    public class InitializeEventApplier : IApplyEvent<HoldHighOrders, InitializeClientOrderEvent>
    {
        public HoldHighOrders Apply(HoldHighOrders state, InitializeClientOrderEvent @event)
        {
            state.ClientId = @event.ClientId.ToString();

            return state;
        }
    }
}