namespace BullOak.Repositories.Appliers
{
    using System.Collections.Generic;

    public interface IApplyEventsToCurrentState<out TState>
    {
        IEnumerable<object> CurrentEvents { get; }

        TState GetCurrentState();
    }
}