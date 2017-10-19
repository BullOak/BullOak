namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class BaseSession<TState> : IManageSessionOf<TState>
        where TState : new()
    {
        //internal readonly ICreateEventAppliers eventApplyFactory;
        internal readonly IEnumerable<IApplyEvents<TState>> appliers;
        internal readonly List<object> eventsToStore = new List<object>(4);
        public IEnumerable<object> NewEvents => eventsToStore.AsReadOnly();

        private struct StateStruct
        {
            public int lastEventCount;
            public TState lastState;

            public StateStruct(int lastEventCount, TState lastSTate)
            {
                this.lastEventCount = lastEventCount;
                this.lastState = lastSTate;
            }
        }
        private StateStruct? lastState = null;
        public TState State
        {
            get
            {
                var state = lastState;
                if (state.HasValue && state.Value.lastEventCount == eventsToStore.Count)
                    return state.Value.lastState;

                StateStruct s = state ?? new StateStruct(0, GetStored());

                if (s.lastEventCount < eventsToStore.Count)
                {
                    foreach (var @event in eventsToStore)
                    {
                        s.lastState = ApplyEvent(s.lastState, @event);
                    }

                    s.lastEventCount += eventsToStore.Count;
                }

                lastState = s;
                return lastState.Value.lastState;
            }
        }

        internal BaseSession(IEnumerable<IApplyEvents<TState>> appliers)
            => this.appliers = appliers ?? throw new ArgumentNullException(nameof(appliers));

        protected abstract TState GetStored();

        //TODO: This method is looping and is already in a loop AND a hot path
        protected TState ApplyEvent(TState state, object @event)
            //=> eventApplyFactory.GetInstance<TState>()
            => appliers
                .First(x => x.CanApplyEvent(@event))
                .Apply(state, @event);

        public abstract Task SaveChanges();

        public void AddToStream(IEnumerable<object> events)
            => eventsToStore.AddRange(events);
        public void AddToStream(params object[] events)
            => eventsToStore.AddRange(events);

        public void Dispose() => eventsToStore.Clear();
    }
}
