namespace BullOak.Repositories.Appliers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.Repositories.Upconverting;

    public struct ApplyResult
    {
        public readonly object State;
        public readonly long? LastEventIndex;
        public readonly bool AnyEventsApplied;

        public ApplyResult(object state, long? lastEventIndex)
        {
            State = state;
            LastEventIndex = lastEventIndex;
            AnyEventsApplied = lastEventIndex.HasValue;
        }
    }

    public interface IApplyEventsToStates
    {
        IEnumerable<Type> SupportedStateTypes { get; }

        ApplyResult Apply(Type stateType, object state, StoredEvent[] events);
        ApplyResult Apply(Type stateType, object state, IEnumerable<StoredEvent> events);
        Task<ApplyResult> Apply(Type stateType, object state, IAsyncEnumerable<StoredEvent> events);

        object Apply(Type stateType, object state, IEnumerable<ItemWithType> events);
        object ApplyEvent(Type stateType, object state, ItemWithType @event);
    }
}
