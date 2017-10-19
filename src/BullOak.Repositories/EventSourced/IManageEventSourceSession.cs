namespace BullOak.Repositories.EventSourced
{
    using System.Collections.Generic;

    public interface IManageEventSourceSession<out TState> : IManageSessionOf<TState>
    {
        IEnumerable<object> EventStream { get; }
    }
}
