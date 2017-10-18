namespace BullOak.Repositories
{
    using System;

    public interface IApplyEvents<TState>
    {
        bool CanApplyEvent(object @event);
        TState Apply(TState state, IHoldEventWithMetadata @event);
    }

    public abstract class BaseApplyEvents<TState, TEvent> : IApplyEvents<TState>
    {
        public bool CanApplyEvent(object @event)
            => @event is TEvent;

        public abstract TState Apply(TState state, IHoldEventWithMetadata<TEvent> @event);

        TState IApplyEvents<TState>.Apply(TState state, IHoldEventWithMetadata @event)
        {
            if (@event is IHoldEventWithMetadata<TEvent> eventEnvelope)
                return Apply(state, eventEnvelope);

            throw new ArgumentException("Argument type not supported", nameof(@event));
        }
    }
}
