namespace BullOak.Repositories
{
    using System;

    public class FuncEventApplier<TState, TEvent> : IApplyEvents<TState>
    {
        private readonly Func<TState, TEvent, TState> applyFunc;

        public bool CanApplyEvent(object @event) => @event is TEvent;
        public FuncEventApplier(Func<TState, TEvent, TState> applyFunc)
            => this.applyFunc = applyFunc ?? throw new ArgumentNullException(nameof(applyFunc));

        public TState Apply(TState state, object @event)
        {
            if (@event is TEvent theEvent) return applyFunc(state, theEvent);

            throw new ArgumentException($"Parameter is not of expected type {typeof(TEvent).Name}", nameof(@event));
        }

        public TState Apply(TState state, TEvent envelope)
            => applyFunc(state, envelope);

        public static implicit operator FuncEventApplier<TState, TEvent>(Func<TState, TEvent, TState> handler)
            => new FuncEventApplier<TState, TEvent>(handler);
    }
}
