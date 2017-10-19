namespace BullOak.Repositories
{
    internal class EmptyApplier<TState> : IApplyEvents<TState>
    {
        public bool CanApplyEvent(object @event) => true;
        public TState Apply(TState state, object @event) => state;
    }
}