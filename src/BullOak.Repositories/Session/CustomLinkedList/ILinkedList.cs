using System.Collections.Generic;

namespace BullOak.Repositories.Session.CustomLinkedList
{
    internal interface ILinkedList<T> : ICollection<T>, IReadOnlyCollection<T>, IEnumerable<T>
    {
        object[] GetBuffer();
    }
}
