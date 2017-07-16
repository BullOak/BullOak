namespace BullOak.Repositories.EventSourced
{
    public interface IReconstituteStateFromEvents<TState>
    {
        bool CanApply(IHoldEventWithMetadata @event);
        TState Apply(TState state, IHoldEventWithMetadata @event);
    }
}
