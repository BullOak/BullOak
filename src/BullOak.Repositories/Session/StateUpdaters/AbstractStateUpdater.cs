namespace BullOak.Repositories.Session.StateUpdaters
{
    using System;
    using System.Collections.Generic;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.StateEmit;

    internal abstract class AbstractStateUpdater<TState, TCollection> : IApplyEventsToCurrentState<TState> 
        where TCollection : IEnumerable<object>
    {
        public static readonly Type TypeOfState = typeof(TState);

        protected TCollection newEventsCollection;
        protected IApplyEventsToStates eventApplier;
        protected bool useThreadSafeOperations;
        protected TState state;
        protected IControlStateWritability<TState> stateMutabilityController;

        public AbstractStateUpdater(IHoldAllConfiguration configuration, TState state, TCollection newEventsCollection)
        {
            eventApplier = configuration.EventApplier;
            useThreadSafeOperations = configuration.ThreadSafetySelector(typeof(TState));
            this.state = state;
            this.newEventsCollection = newEventsCollection;

            stateMutabilityController = state is ICanSwitchBackAndToReadOnly
                ? (IControlStateWritability<TState>) new StateController<TState>(state)
                : new DoNothingController<TState>(state);
        }

        public IEnumerable<object> CurrentEvents => newEventsCollection;

        public abstract TState GetCurrentState();
    }
}