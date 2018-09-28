namespace BullOak.Repositories.Appliers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.StateEmit;

    internal class EventApplier : IApplyEventsToStates
    {
        private List<ApplierRetriever> unindexedAppliers = new List<ApplierRetriever>();

        private IDictionary<EventAndStateTypes, ApplierRetriever> indexedStateAppliers =
            new Dictionary<EventAndStateTypes, ApplierRetriever>();

        public IEnumerable<Type> SupportedStateTypes { get; private set; } = new List<Type>();

        internal void SeedWith(ICollection<ApplierRetriever> allAppliers)
        {
            unindexedAppliers.AddRange(allAppliers);
            SupportedStateTypes = unindexedAppliers.Select(x => x.StateType);
        }

        public object Apply(Type stateType, object state, IEnumerable<ItemWithType> events)
        {
            var switchable = state as ICanSwitchBackAndToReadOnly;
            if (switchable != null) switchable.CanEdit = true;

            foreach(var @event in events)
            {
                state = ApplyAssumeWritable(stateType, state, @event.type, @event.instance);
            }

            if (switchable != null) switchable.CanEdit = false;

            return state;
        }

        public object Apply(Type stateType, object state, ItemWithType[] events)
        {
            var switchable = state as ICanSwitchBackAndToReadOnly;
            if (switchable != null) switchable.CanEdit = true;

            int length = events.Length;
            ItemWithType @event;
            for (int i = 0; i < length; i++)
            {
                @event = events[i];
                state = ApplyAssumeWritable(stateType, state, @event.type, @event.instance);
            }

            if (switchable != null) switchable.CanEdit = false;

            return state;
        }

        public object ApplyEvent(Type stateType, object state, ItemWithType @event)
        {
            var switchable = state as ICanSwitchBackAndToReadOnly;
            if (switchable != null) switchable.CanEdit = true;

            state = ApplyAssumeWritable(stateType, state, @event.type, @event.instance);

            if (switchable != null) switchable.CanEdit = false;

            return state;
        }

        private object ApplyAssumeWritable(Type stateType, object state, Type eventType, object @event)
        {
            var key = new EventAndStateTypes(stateType, eventType);

            ApplierRetriever indexedApplier;

            if (!indexedStateAppliers.TryGetValue(key, out indexedApplier))
            {
                lock (indexedStateAppliers)
                {
                    if (!indexedStateAppliers.TryGetValue(key, out indexedApplier))
                    {
                        indexedApplier = GetApplierFromUnindexed(key);

                        indexedStateAppliers.Add(key, indexedApplier);
                    }
                }
            }

            return indexedApplier.GetApplier()
                .Apply(state, @event);
        }

        private ApplierRetriever GetApplierFromUnindexed(EventAndStateTypes index)
        {
            var applier = unindexedAppliers
                .FirstOrDefault(x => x.GetApplier(index)?.CanApplyEvent(index.stateType, index.eventType) == true);

            if (applier.IsDefault) throw new ApplierNotFoundException(index.stateType, index.eventType);

            return applier;
        }
    }
}
