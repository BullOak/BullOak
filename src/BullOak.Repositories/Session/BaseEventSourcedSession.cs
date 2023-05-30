namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Upconverting;

    public abstract class BaseEventSourcedSession<TState, TConcurrencyId> : BaseRepoSession<TState>
    {
        private static readonly Type typeOfState = typeof(TState);

        private TConcurrencyId concurrencyId;
        public TConcurrencyId ConcurrencyId => concurrencyId;

        protected readonly IApplyEventsToStates eventApplier;

        public BaseEventSourcedSession(IValidateState<TState> validator, IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(validator, configuration, disposableHandle)
            => eventApplier = configuration.EventApplier;

        public BaseEventSourcedSession(IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(configuration, disposableHandle)
            => eventApplier = configuration.EventApplier;

        public void LoadFromEvents(ItemWithType[] storedEvents, TConcurrencyId concurrencyId)
            => LoadFromEvents(storedEvents, storedEvents.Length == 0, concurrencyId);

        public void LoadFromEvents(IEnumerable<ItemWithType> storedEvents, bool isNew, TConcurrencyId concurrencyId)
        {
            var initialState = configuration.StateRehydrator.RehydrateFrom<TState>(storedEvents);

            Initialize((TState)initialState, isNew);
            this.concurrencyId = concurrencyId;
        }
    }
}
