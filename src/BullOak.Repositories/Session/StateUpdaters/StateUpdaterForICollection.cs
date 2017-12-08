namespace BullOak.Repositories.Session.StateUpdaters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class StateUpdaterForICollection<TState> : AbstractStateUpdater<TState, ICollection<object>>
    {
        public StateUpdaterForICollection(IHoldAllConfiguration configuration, TState state, ICollection<object> newEventsCollection)
            :base(configuration, state, newEventsCollection)
        { }

        public sealed override TState GetCurrentState()
        {
            bool isLockTaken = false;
            try
            {
                if (useThreadSafeOperations) Monitor.Enter(newEventsCollection, ref isLockTaken);
                stateMutabilityController.MakeStateWritable();
                state = newEventsCollection.Aggregate(state, eventApplier.Apply);
                stateMutabilityController.MakeStateReadOnly();
                return state;
            }
            finally
            {
                if (useThreadSafeOperations && isLockTaken) Monitor.Enter(newEventsCollection);
            }
        }
    }
}