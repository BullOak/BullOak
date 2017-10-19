namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.EventStream;

    public interface IManageSessionOf<out TState> : IDisposable
    {
        IEnumerable<object> NewEvents { get; }

        TState State { get; }

        Task SaveChanges();

        void AddToStream(params object[] events);
        void AddToStream(IEnumerable<object> events);
    }
}