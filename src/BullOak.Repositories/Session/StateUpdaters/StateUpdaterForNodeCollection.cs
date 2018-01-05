namespace BullOak.Repositories.Session.StateUpdaters
{
    using System;
    using System.Threading;
    using BullOak.Repositories.Session.CustomLinkedList;

    internal class StateUpdaterForLinkedList<TState> : AbstractStateUpdater<TState, ILinkedList<object>>
    {
        private readonly ILinkedList<object> newEventsCollection;
        private NodeEnumerator<object> enumerator;
        public StateUpdaterForLinkedList(IHoldAllConfiguration configuration, TState state, ILinkedList<object> newEventsCollection) 
            : base(configuration, state, newEventsCollection)
            => this.newEventsCollection = newEventsCollection;
            //=> newEventsEnumerator = newEventsCollection.GetEnumerator() as NodeEnumerator<object> ?? throw new Exception("Unexpected enumerator type.");

        public sealed override TState GetCurrentState()
        {
            if (newEventsCollection.Count == 0) return state;

            if (enumerator == null)
                enumerator = newEventsCollection.GetEnumerator() as NodeEnumerator<object> ?? throw new Exception("Unexpected enumerator type.");

            if (enumerator.Current is Node<object> node && node.next == null) return state;

            bool isLockTaken = false;
            try
            {
                if (useThreadSafeOperations) Monitor.Enter(newEventsCollection, ref isLockTaken);
                while (enumerator.MoveNext())
                {
                    state = (TState)eventApplier.ApplyEvent(TypeOfState, state, enumerator.Current);
                }
            }
            finally
            {
                if (useThreadSafeOperations && isLockTaken) Monitor.Exit(newEventsCollection);
            }
            return state;
        }
    }
}
