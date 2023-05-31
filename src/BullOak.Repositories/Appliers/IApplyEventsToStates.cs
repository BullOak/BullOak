namespace BullOak.Repositories.Appliers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.Repositories.Upconverting;

    public interface IApplyEventsToStates
    {
        IEnumerable<Type> SupportedStateTypes { get; }

        object Apply(Type stateType, object state, ItemWithType[] events);
        object Apply(Type stateType, object state, IEnumerable<ItemWithType> events);
        Task<object> Apply(Type stateType, object state, IAsyncEnumerable<ItemWithType> events);
        object ApplyEvent(Type stateType, object state, ItemWithType @event);
    }
}
