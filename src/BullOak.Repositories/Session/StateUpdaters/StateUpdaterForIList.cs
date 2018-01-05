namespace BullOak.Repositories.Session.StateUpdaters
{
    using System.Collections.Generic;
    using System.Threading;

    internal class StateUpdaterForIList<TState> : AbstractStateUpdater<TState, IList<object>>
    {
        private int currentIndex = 0;

        public StateUpdaterForIList(IHoldAllConfiguration configuration, TState state, IList<object> newEventsCollection)
            :base(configuration, state, newEventsCollection)
        { }

        public sealed override TState GetCurrentState()
        {
            if (currentIndex == newEventsCollection.Count) return state;
            bool isLockTaken = false;
            try
            {
                if (useThreadSafeOperations) Monitor.Enter(newEventsCollection, ref isLockTaken);
                stateMutabilityController.MakeStateWritable();
                for (; currentIndex < newEventsCollection.Count; currentIndex++)
                    state = eventApplier.Apply(state, newEventsCollection[currentIndex]);
                stateMutabilityController.MakeStateReadOnly();
            }
            finally
            {
                if (useThreadSafeOperations && isLockTaken) Monitor.Enter(newEventsCollection);
            }
            return state;
        }
    }
}