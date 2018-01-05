namespace BullOak.Repositories.Appliers
{
    using System;
    using System.Collections.Generic;

    public interface IApplyEventsToStates
    {
        IEnumerable<Type> SupportedStateTypes { get; }
        TState Apply<TState>(TState state, object @event);
        object Apply(Type stateType, object state, Type eventType, object @event);
    }
}