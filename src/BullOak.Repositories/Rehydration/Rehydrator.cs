namespace BullOak.Repositories.Rehydration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Rehydrator : IRehydrateState
    {
        private readonly IHoldAllConfiguration config;

        public Rehydrator(IHoldAllConfiguration config)
            => this.config = config ?? throw new ArgumentNullException(nameof(config));

        public TState RehydrateFrom<TState>(IEnumerable<ItemWithType> events, TState initialState = default)
        {
            events = config.EventUpconverter.Upconvert(events);
            (Type stateType, TState initial) = Setup(initialState);

            return (TState)config.EventApplier.Apply(stateType, initial, events);
        }

        public async Task<TState> RehydrateFrom<TState>(IAsyncEnumerable<ItemWithType> events, TState initialState = default)
        {
            events = config.EventUpconverter.Upconvert(events);
            (Type stateType, TState initial) = Setup(initialState);

            return (TState)(await config.EventApplier.Apply(stateType, initial, events));
        }

        private (Type stateType, TState initialState) Setup<TState>(TState initialState = default)
        {
            var stateType = typeof(TState);

            if (initialState == null)
                initialState = (TState)config.StateFactory.GetState(stateType);

            return (stateType, initialState);
        }
    }
}
