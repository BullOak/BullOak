namespace BullOak.Repositories.Session.StateUpdaters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class StateUpdaterForIEnumerable<TState> : AbstractStateUpdater<TState, IEnumerable<object>>
    {
        public StateUpdaterForIEnumerable(IHoldAllConfiguration configuration, TState state, IEnumerable<object> newEventsCollection)
            :base(configuration, state, newEventsCollection)
        { }

        public sealed override TState GetCurrentState()
        {
            bool isLockTaken = false;
            try
            {
                if (useThreadSafeOperations) Monitor.Enter(newEventsCollection, ref isLockTaken);
                state = (TState) eventApplier.Apply(TypeOfState, state, newEventsCollection);
                return state;
            }
            finally
            {
                if (useThreadSafeOperations && isLockTaken) Monitor.Enter(newEventsCollection);
            }
        }
    }
}