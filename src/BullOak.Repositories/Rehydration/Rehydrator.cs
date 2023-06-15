namespace BullOak.Repositories.Rehydration
{
    using BullOak.Repositories.Appliers;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Rehydrator : IRehydrateState
    {
        private readonly IHoldAllConfiguration config;

        public Rehydrator(IHoldAllConfiguration config)
            => this.config = config ?? throw new ArgumentNullException(nameof(config));

        public RehydrateFromResult<TState> RehydrateFrom<TState>(IEnumerable<StoredEvent> events, TState initialState = default)
        {
            events = Upconvert(events);
            (Type stateType, TState initial) = Setup(initialState);

            var applyResult = config.EventApplier.Apply(stateType, initial, events);

            return new RehydrateFromResult<TState>((TState)applyResult.State, !applyResult.AnyEventsApplied, applyResult.LastEventIndex);
        }

        public async Task<RehydrateFromResult<TState>> RehydrateFrom<TState>(IAsyncEnumerable<StoredEvent> events, TState initialState = default)
        {
            events = Upconvert(events);
            (Type stateType, TState initial) = Setup(initialState);

            var applyResult = await config.EventApplier.Apply(stateType, initial, events);

            return new RehydrateFromResult<TState>((TState)applyResult.State, !applyResult.AnyEventsApplied, applyResult.LastEventIndex);
        }

        private (Type stateType, TState initialState) Setup<TState>(TState initialState = default)
        {
            var stateType = typeof(TState);

            if (initialState == null)
                initialState = (TState)config.StateFactory.GetState(stateType);

            return (stateType, initialState);
        }

        private IEnumerable<StoredEvent> Upconvert(IEnumerable<StoredEvent> storedEvents)
        {
            foreach (var se in storedEvents)
            {
                var results = config.EventUpconverter.Upconvert(se.ToItemWithType());

                foreach (var upconverted in results)
                    yield return StoredEvent.FromItemWithType(upconverted, se.EventIndex);
            }
        }

        private async IAsyncEnumerable<StoredEvent> Upconvert(IAsyncEnumerable<StoredEvent> storedEvents)
        {
            await foreach (var se in storedEvents)
            {
                var results = config.EventUpconverter.Upconvert(se.ToItemWithType());

                foreach (var upconverted in results)
                    yield return StoredEvent.FromItemWithType(upconverted, se.EventIndex);
            }
        }
    }
}
