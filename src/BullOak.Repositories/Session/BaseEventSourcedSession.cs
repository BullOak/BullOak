namespace BullOak.Repositories.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Upconverting;

    public abstract class BaseEventSourcedSession<TState> : BaseRepoSession<TState>
    {
        private static readonly Type typeOfState = typeof(TState);

        private long concurrencyId;
        public long ConcurrencyId => concurrencyId;

        protected readonly IApplyEventsToStates eventApplier;

        public BaseEventSourcedSession(IValidateState<TState> validator, IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(validator, configuration, disposableHandle)
            => eventApplier = configuration.EventApplier;

        public BaseEventSourcedSession(IHoldAllConfiguration configuration, IDisposable disposableHandle = null)
            : base(configuration, disposableHandle)
            => eventApplier = configuration.EventApplier;

        public void LoadFromEvents(StoredEvent[] storedEvents)
        {
            var rehydrateResult = configuration.StateRehydrator.RehydrateFrom<TState>(storedEvents);

            Initialize(rehydrateResult.State, !rehydrateResult.LastEventIndex.HasValue);
            this.concurrencyId = rehydrateResult.LastEventIndex ?? -1;
        }

        public void LoadFromEvents(IEnumerable<StoredEvent> storedEvents)
        {
            var rehydrateResult = configuration.StateRehydrator.RehydrateFrom<TState>(storedEvents);

            Initialize(rehydrateResult.State, !rehydrateResult.LastEventIndex.HasValue);
            this.concurrencyId = rehydrateResult.LastEventIndex ?? -1;
        }

        public Task LoadFromEvents(IAsyncEnumerable<StoredEvent> storedEvents)
            => LoadFromEventsInternal(storedEvents);

        public async Task LoadFromEvents(IAsyncEnumerable<StoredEvent> storedEvents, Func<long> concurrencyIdFunc)
        {
            await LoadFromEventsInternal(storedEvents);
            this.concurrencyId = concurrencyIdFunc();
        }

        private async Task LoadFromEventsInternal(IAsyncEnumerable<StoredEvent> storedEvents)
        {
            var rehydrateResult = await configuration.StateRehydrator.RehydrateFrom<TState>(storedEvents);

            Initialize(rehydrateResult.State, !rehydrateResult.LastEventIndex.HasValue);
            this.concurrencyId = rehydrateResult.LastEventIndex ?? -1;
        }
    }
}
