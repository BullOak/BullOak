namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;
    using BullOak.Repositories.Appliers;

    public class StateApplier : IApplyEvent<IHoldHigherOrder, MyEvent>,
        IApplyEvent<IHoldHigherOrder, BuyerFullNameSetEvent>,
        IApplyEvent<IHoldHigherOrder, BalanceSetEvent>,
        IApplyEvent<IHoldHigherOrder, TimeOfLastBalanceUpdateSetEvent>
    {
        public IHoldHigherOrder Apply(IHoldHigherOrder state, MyEvent @event)
        {
            state.HigherOrder = Math.Max(@event.Order, state.HigherOrder);

            return state;
        }

        public IHoldHigherOrder Apply(IHoldHigherOrder state, BuyerFullNameSetEvent @event)
        {
            state.FullName = @event.FullName;
            return state;
        }

        public IHoldHigherOrder Apply(IHoldHigherOrder state, BalanceSetEvent @event)
        {
            state.LastBalance = @event.CurrentBalance;
            return state;
        }

        public IHoldHigherOrder Apply(IHoldHigherOrder state, TimeOfLastBalanceUpdateSetEvent @event)
        {
            state.BalaceUpdateTime = @event.LastUpdateTimeStamp;
            return state;
        }
    }
}
