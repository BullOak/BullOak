namespace BullOak.Repositories.Appliers
{
    using System;
    using System.Collections.Generic;

    public interface IApplyEventsToStates
    {
        IEnumerable<Type> SupportedStateTypes { get; }

        object Apply(Type stateType, object state, object[] events);
        object Apply(Type stateType, object state, IEnumerable<object> events);
        object ApplyEvent(Type stateType, object state, object @event);
    }
}