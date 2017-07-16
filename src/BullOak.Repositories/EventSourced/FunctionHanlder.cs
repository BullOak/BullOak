namespace BullOak.Repositories.EventSourced
{
    using System;

    public sealed class FunctionHanlder<TState, TEvent> : BaseHandler<TState, TEvent>
    {
        private readonly Func<TState, TEvent, TState> handler;

        public FunctionHanlder(Func<TState, TEvent, TState> handler)
        {
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        protected override TState Apply(TState state, TEvent @event) => handler(state, @event);
    }
}
