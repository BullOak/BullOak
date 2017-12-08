namespace BullOak.Repositories.Appliers
{
    using System;

    public abstract class BaseApplyEvents<TState, TEvent> : IApplyEvents<TState>,
        IApplyEvent<TState, TEvent>
    {
        bool IApplyEvents<TState>.CanApplyEvent(object @event)
            => @event is TEvent;

        public abstract TState Apply(TState state, TEvent @event);

        TState IApplyEvents<TState>.Apply(TState state, object @event)
        {
            if (@event is TEvent theEvent)
                return Apply(state, theEvent);

            throw new ArgumentException("Argument type not supported", nameof(@event));
        }
    }
}