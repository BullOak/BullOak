namespace BullOak.Repositories.Session
{
    using System.Collections.Generic;

    public interface IAddEventsToSession
    {
        IEnumerable<object> NewEvents { get; }

        void AddToStream(IEnumerable<object> events);
        void AddToStream(object @event);
        void AddToStream(object[] events);

        void Clear();
    }
}