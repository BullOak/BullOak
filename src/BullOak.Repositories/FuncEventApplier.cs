namespace BullOak.Repositories
{
    using System;

    public class FuncEventApplier<TState, TEvent> : IApplyEvents<TState>
    {
        private readonly Func<TState, IHoldEventWithMetadata<TEvent>, TState> applyFunc;

        public bool CanApplyEvent(object @event) => @event is TEvent;

        public FuncEventApplier(Func<TState, IHoldEventWithMetadata<TEvent>, TState> applyFunc)
            => this.applyFunc = applyFunc ?? throw new ArgumentNullException(nameof(applyFunc));

        public TState Apply(TState state, IHoldEventWithMetadata envelope)
            => applyFunc(state, envelope as IHoldEventWithMetadata<TEvent>);

        public static implicit operator FuncEventApplier<TState, TEvent>(Func<TState, IHoldEventWithMetadata<TEvent>, TState> handler)
            => new FuncEventApplier<TState, TEvent>(handler);
    }
}
