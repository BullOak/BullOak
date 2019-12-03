namespace BullOak.Repositories
{
    using System;
    using BullOak.Repositories.Appliers;

    internal class EmptyApplier<TState> : IApplyEvents<TState>
    {
        public bool CanApplyEvent(Type eventType) => true;
        public TState Apply(TState state, object @event) => state;
    }
}
