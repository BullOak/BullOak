namespace BullOak.Repositories.Session
{
    using System;
    using System.Linq;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Upconverting;

    public abstract class BaseEventSourcedSession<TState, TConcurrencyId> : BaseRepoSession<TState>
    {
        private static readonly Type typeOfState = typeof(TState);

        private TConcurrencyId concurrencyId;
        public TConcurrencyId ConcurrencyId => concurrencyId;

        protected readonly IApplyEventsToStates eventApplier;

        public BaseEventSourcedSession(IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(configuration, disposableHandle)
            => eventApplier = configuration.EventApplier;

        public void LoadFromEvents(ItemWithType[] storedEvents, TConcurrencyId concurrencyId)
        {
            var initialState = configuration.StateRehydrator.RehydrateFrom<TState>(storedEvents);

            Initialize((TState) initialState, storedEvents.Length == 0);
            this.concurrencyId = concurrencyId;
        }
    }
}