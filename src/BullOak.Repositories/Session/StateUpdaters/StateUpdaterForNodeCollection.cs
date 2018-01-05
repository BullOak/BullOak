namespace BullOak.Repositories.Session.StateUpdaters
{
    using System;
    using System.Threading;
    using BullOak.Repositories.Session.CustomLinkedList;

    internal class StateUpdaterForLinkedList<TState> : AbstractStateUpdater<TState, ILinkedList<object>>
    {
        private readonly NodeEnumerator<object> newEventsEnumerator;

        public StateUpdaterForLinkedList(IHoldAllConfiguration configuration, TState state, ILinkedList<object> newEventsCollection) 
            : base(configuration, state, newEventsCollection)
            => newEventsEnumerator = newEventsCollection.GetEnumerator() as NodeEnumerator<object> ?? throw new Exception("Unexpected enumerator type.");

        public sealed override TState GetCurrentState()
        {
            if (newEventsEnumerator.Current is Node<object> node && node.next == null) return state;

            bool isLockTaken = false;
            try
            {
                if (useThreadSafeOperations) Monitor.Enter(newEventsCollection, ref isLockTaken);
                stateMutabilityController.MakeStateWritable();
                while (newEventsEnumerator.MoveNext())
                {
                    state = eventApplier.Apply(state, newEventsEnumerator.Current);
                }
                stateMutabilityController.MakeStateReadOnly();
            }
            finally
            {
                if (useThreadSafeOperations && isLockTaken) Monitor.Exit(newEventsCollection);
            }
            return state;
        }
    }
}
