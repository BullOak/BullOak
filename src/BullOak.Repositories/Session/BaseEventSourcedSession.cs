namespace BullOak.Repositories.Session
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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

        public void LoadFromEvents(object[] storedEvents, TConcurrencyId concurrencyId)
        {
            var eventsWithTypes = storedEvents
                .Select(x => new ItemWithType(x))
                .ToArray();

            var upconvertedEvents = configuration
                .EventUpconverter
                .Upconvert(eventsWithTypes);

            var initialState = configuration.StateFactory.GetState(typeOfState);

            initialState = eventApplier.Apply(typeOfState, initialState, upconvertedEvents);

            Initialize((TState) initialState, storedEvents.Length == 0);
            this.concurrencyId = concurrencyId;
        }
    }
}