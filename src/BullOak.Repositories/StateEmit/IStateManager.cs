namespace BullOak.Repositories.StateEmit
{
    using System;
    using System.Linq.Expressions;

    public interface IManageState<TState>
    {
        TState State { get; }

        TPropertyType GetInstanceOfEntityState<TPropertyType>(Expression<Func<TPropertyType>> propertySelector);
    }

    internal interface IManageStateAndApplyEvents<TState> : IManageState<TState>
    {
        void Apply(object @event);
        void Apply(object[] events);
    }

    internal class NormalStateManager<TState> : IManageStateAndApplyEvents<TState>
        where TState : class
    {
        public TState State { get; private set; }

        public NormalStateManager(TState state)
        {
            if (state is ICanSwitchBackAndToReadOnly)
                throw new ArgumentException(
                    $"Read-Write switchable states should be managed by {nameof(EmittedWriteLockableStateManager<TState>)}",
                    nameof(state));

            State = state;
        }


        public void Apply(object @event)
        {
            throw new NotImplementedException();
        }

        public void Apply(object[] events)
        {
            throw new NotImplementedException();
        }

        public TPropertyType GetInstanceOfEntityState<TPropertyType>(Expression<Func<TPropertyType>> propertySelector)
        {
            throw new NotImplementedException();
        }
    }

    internal class EmittedWriteLockableStateManager<TState> : IManageStateAndApplyEvents<TState>
        where TState : class
    {
        public TState State { get; private set; }

        public EmittedWriteLockableStateManager(TState state)
        {
            if (!(state is ICanSwitchBackAndToReadOnly))
                throw new ArgumentException(
                    $"Read-Write switchable states should be managed by {nameof(NormalStateManager<TState>)}",
                    nameof(state));

            State = state;
        }


        public void Apply(object @event)
        {
            throw new NotImplementedException();
        }

        public void Apply(object[] events)
        {
            throw new NotImplementedException();
        }

        public TPropertyType GetInstanceOfEntityState<TPropertyType>(Expression<Func<TPropertyType>> propertySelector)
        {
            throw new NotImplementedException();
        }
    }
}
