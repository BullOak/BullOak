namespace BullOak.Repositories.Appliers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.Repositories.Upconverting;

    public struct ApplyResult
    {
        public readonly bool IsStateDefault;
        public readonly object State;

        public ApplyResult(object state, bool isStateDefault)
        {
            State = state;
            IsStateDefault = isStateDefault;
        }
    }

    public interface IApplyEventsToStates
    {
        IEnumerable<Type> SupportedStateTypes { get; }

        ApplyResult Apply(Type stateType, object state, ItemWithType[] events);
        ApplyResult Apply(Type stateType, object state, IEnumerable<ItemWithType> events);
        Task<ApplyResult> Apply(Type stateType, object state, IAsyncEnumerable<ItemWithType> events);
        object ApplyEvent(Type stateType, object state, ItemWithType @event);
    }
}
