namespace BullOak.Repositories.Appliers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.StateEmit;

    internal struct ApplierRetriever
    {
        private static Type typeOfSwitchableInterface = typeof(ICanSwitchBackAndToReadOnly);

        private Type stateType;
        private bool singleInstance;
        private IApplyEventsInternal applierInstance;
        private Func<IApplyEventsInternal> applierFactory;
        private bool isStateTypeSwitchable;

        public Type StateType => stateType;
        public bool SingleInstance => singleInstance;
        public IApplyEventsInternal ApplierInstance => applierInstance;
        public Func<IApplyEventsInternal> ApplierFactory => applierFactory;
        public bool IsStateTypeSwitchable => isStateTypeSwitchable;
        public bool IsDefault => stateType == null;

        public ApplierRetriever(Type stateType, IApplyEventsInternal applier)
            : this(stateType)
        {
            singleInstance = true;
            applierInstance = applier;
        }

        public ApplierRetriever(Type stateType, Func<IApplyEventsInternal> applierFactory)
            :this(stateType)
        {
            singleInstance = false;
            this.applierFactory = applierFactory;
        }

        private ApplierRetriever(Type stateType)
        {
            isStateTypeSwitchable = GetIfStateSwitchable(stateType);
            this.stateType = stateType;
            singleInstance = false;
            applierInstance = null;
            applierFactory = null;
        }

        private static bool GetIfStateSwitchable(Type state)
            => state.GetInterfaces().Any(x => ReferenceEquals(x, typeOfSwitchableInterface));

        internal IApplyEventsInternal GetApplier()
        {
            if (singleInstance) return applierInstance;
            return applierFactory();
        }

        public IApplyEventsInternal GetApplier(EventAndStateTypes types, bool withCheck = false)
        {
            var applier = GetApplier();

            if (withCheck && !applier.CanApplyEvent(types.stateType, types.eventType))
                throw new ArgumentException(
                    $"Type of event {types.eventType} is not supported. Applier type: {applier.GetType().Name}");

            return applier;
        }
    }

    internal class EventAndStateTypes : IEquatable<EventAndStateTypes>
    {
        public Type stateType;
        public Type eventType;
        private int hashCode;

        public EventAndStateTypes(Type stateType, Type eventType)
        {
            this.stateType = stateType;
            this.eventType = eventType;

            unchecked
            {
                hashCode = (stateType.GetHashCode() * 397) ^ eventType.GetHashCode();
            }
        }

        /// <inheritdoc />
        public bool Equals(EventAndStateTypes other) 
            => ReferenceEquals(stateType, other.stateType) && ReferenceEquals(eventType, other.eventType);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EventAndStateTypes) obj);
        }

        public override int GetHashCode() => hashCode;
    }

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

        public object Apply(Type stateType, object state, IEnumerable<object> events)
        {
            var switchable = state as ICanSwitchBackAndToReadOnly;
            if (switchable != null) switchable.CanEdit = true;

            foreach(var @event in events)
            {
                state = ApplyAssumeWritable(stateType, state, @event.GetType(), @event);
            }

            if (switchable != null) switchable.CanEdit = false;

            return state;
        }

        public object Apply(Type stateType, object state, object[] events)
        {
            var switchable = state as ICanSwitchBackAndToReadOnly;
            if (switchable != null) switchable.CanEdit = true;

            int length = events.Length;
            object @event;
            for (int i = 0; i < length; i++)
            {
                @event = events[i];
                state = ApplyAssumeWritable(stateType, state, @event.GetType(), @event);
            }

            if (switchable != null) switchable.CanEdit = false;

            return state;
        }

        public object ApplyEvent(Type stateType, object state, object @event)
        {
            var switchable = state as ICanSwitchBackAndToReadOnly;
            if (switchable != null) switchable.CanEdit = true;

            state = ApplyAssumeWritable(stateType, state, @event.GetType(), @event);

            if (switchable != null) switchable.CanEdit = false;

            return state;
        }

        private object ApplyAssumeWritable(Type stateType, object state, Type eventType, object @event)
        {
            var key = new EventAndStateTypes(stateType, eventType);
            
            return GetApplierRetrieverFor(key)
                .GetApplier()
                .Apply(state, @event);
        }

        private ApplierRetriever GetApplierRetrieverFor(EventAndStateTypes index)
        {
            if (!indexedStateAppliers.TryGetValue(index, out var indexedApplier))
            {
                indexedApplier = GetApplierFromUnindexed(index);

                indexedStateAppliers.Add(index, indexedApplier);
            }

            return indexedApplier;
        }

        private ApplierRetriever GetApplierFromUnindexed(EventAndStateTypes index)
        {
            var applier = unindexedAppliers
                .FirstOrDefault(x => x.GetApplier(index)?.CanApplyEvent(index.stateType, index.eventType) == true);

            if (applier.IsDefault) throw new ApplierNotFoundException(index.stateType, index.eventType);

            return applier;
        }
    }

    internal class ApplierNotFoundException : Exception
    {
        public ApplierNotFoundException(Type typeOfState, Type typeOfEvent)
            : base($"Applier for event {typeOfEvent.Name} for state {typeOfState.Name} was not found or registered.")
        { }
        public ApplierNotFoundException(Type typeOfState)
            : base($"No appliers where found for state {typeOfState.Name}.")
        { }
    }
}
