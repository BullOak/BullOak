namespace BullOak.Repositories.Appliers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal struct ApplierRetriever
    {
        private Type stateType;
        private bool singleInstance;
        private IApplyEventsInternal applierInstance;
        private Func<IApplyEventsInternal> applierFactory;

        public Type StateType => stateType;
        public bool SingleInstance => singleInstance;
        public IApplyEventsInternal ApplierInstance => applierInstance;
        public Func<IApplyEventsInternal> ApplierFactory => applierFactory;
        public bool IsDefault => stateType == null;

        public ApplierRetriever(Type stateType, IApplyEventsInternal applier)
        {
            this.stateType = stateType;
            singleInstance = true;
            applierFactory = null;
            applierInstance = applier;
        }

        public ApplierRetriever(Type stateType, Func<IApplyEventsInternal> applierFactory)
        {
            this.stateType = stateType;
            singleInstance = false;
            applierInstance = null;
            this.applierFactory = applierFactory;
        }

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

        public TState Apply<TState>(TState state, object @event)
            => (TState)Apply(typeof(TState), state, @event.GetType(), @event);

        public object Apply(Type stateType, object state, Type eventType, object @event)
        {
            var key = new EventAndStateTypes(stateType, eventType);
            var applier = GetApplierFor(key);
            return applier.Apply(state, @event);
        }

        private IApplyEventsInternal GetApplierFor(EventAndStateTypes index)
        {
            if (!indexedStateAppliers.TryGetValue(index, out var indexedApplier))
            {
                indexedApplier = GetApplierFromUnindexed(index);

                indexedStateAppliers.Add(index, indexedApplier);
            }

            return indexedApplier.GetApplier(index);
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
