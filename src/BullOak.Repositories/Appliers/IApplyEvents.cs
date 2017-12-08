namespace BullOak.Repositories.Appliers
{
    public interface IApplyEvents<TState>
    {
        bool CanApplyEvent(object @event);
        TState Apply(TState state, object @event);
    }

    public interface IApplyEvent<TState, in TEvent>
    {
        TState Apply(TState state, TEvent @event);
    }
}