namespace BullOak.Common
{
    using System;

    public interface ITransitionTo<T>
    {
        bool CanTransitionTo(T newState);
    }
}
