namespace BullOak.Repositories.Appliers
{
    using System;
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
}
