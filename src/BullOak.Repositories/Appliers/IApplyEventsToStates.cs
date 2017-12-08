namespace BullOak.Repositories.Appliers
{
    public interface IApplyEventsToStates
    {
        TState Apply<TState, TEvent>(TState state, TEvent @event);
    }
}