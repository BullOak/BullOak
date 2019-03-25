namespace BullOak.Repositories.Rehydration
{
    using System;
    using System.Collections.Generic;

    public class Rehydrator : IRehydrateState
    {
        private readonly IHoldAllConfiguration config;

        public Rehydrator(IHoldAllConfiguration config)
            => this.config = config ?? throw new ArgumentNullException(nameof(config));

        public TState RehydrateFrom<TState>(IEnumerable<ItemWithType> events, TState initialState = default)
        {
            var stateType = typeof(TState);
            events = config.EventUpconverter.Upconvert(events);

            if (initialState == null)
                initialState = (TState)config.StateFactory.GetState(stateType);

            return (TState) config.EventApplier.Apply(stateType, initialState, events);
        }
    }
}
