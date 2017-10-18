namespace BullOak.Repositories.EventSourced
{
    using System;

    public sealed class FunctionHanlder<TState, TEvent> : IApplyEvents<TState>
        where TState : new()
    {
        private readonly Func<TState, TEvent, TState> handler;

        private FunctionHanlder(Func<TState, TEvent, TState> handler)
            => this.handler = handler ?? throw new ArgumentNullException(nameof(handler));

        public bool CanApplyEvent(object @event)
            => @event is TEvent;

        public TState Apply(TState state, IHoldEventWithMetadata @event)
            => handler(state, ((IHoldEventWithMetadata<TEvent>)@event).Event);

        public static explicit operator FunctionHanlder<TState, TEvent>(Func<TState, TEvent, TState> handler)
            => new FunctionHanlder<TState, TEvent>(handler);
    }
}
