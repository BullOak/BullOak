namespace BullOak.Repositories
{
    using System;

    internal struct EventAndStateTypeInfo
    {
        public readonly Type eventType;
        public readonly Type stateType;

        private EventAndStateTypeInfo(Type eventType, Type stateType)
        {
            this.eventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            this.stateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
        }

        public static EventAndStateTypeInfo GetFor<TState, TEvent>()
        {
            return new EventAndStateTypeInfo(typeof(TState), typeof(TEvent));
        }
    }
}
