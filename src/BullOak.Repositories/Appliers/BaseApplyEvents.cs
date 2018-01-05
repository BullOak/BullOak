namespace BullOak.Repositories.Appliers
{
    public abstract class BaseApplyEvents<TState, TEvent> : IApplyEvent<TState, TEvent>
    {
        public abstract TState Apply(TState state, TEvent @event);
    }
}