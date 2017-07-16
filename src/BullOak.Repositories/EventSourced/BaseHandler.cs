namespace BullOak.Repositories.EventSourced
{
    using System;

    public abstract class BaseHandler<TState, TEvent> : IReconstituteStateFromEvents<TState>
    {
        public bool CanApply(IHoldEventWithMetadata @event) => @event.EventType == typeof(TEvent);

        TState IReconstituteStateFromEvents<TState>.Apply(TState state, IHoldEventWithMetadata eventEnvelope)
        {
            if (CanApply(eventEnvelope))
            {
                return Apply(state, (TEvent)eventEnvelope.Event);
            }

            throw new ArgumentException($"Event envelope is not the correct type. Expecting enveloper with event of type {typeof(TEvent).AssemblyQualifiedName} but received envelope with event of type {eventEnvelope.EventType.AssemblyQualifiedName}");
        }

        protected abstract TState Apply(TState state, TEvent @event);
    }
}
