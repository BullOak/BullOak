namespace BullOak.Repositories
{
    using System;

    public interface IApplyEvents<TState>
    {
        bool CanApplyEvent(object @event);
        TState Apply(TState state, object @event);
    }

    public abstract class BaseApplyEvents<TState, TEvent> : IApplyEvents<TState>
    {
        public bool CanApplyEvent(object @event)
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
