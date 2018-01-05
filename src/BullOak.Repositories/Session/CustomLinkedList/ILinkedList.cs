using System.Collections.Generic;

namespace BullOak.Repositories.Session.CustomLinkedList
{
    internal interface ILinkedList<T> : ICollection<T>, IReadOnlyCollection<T>, IEnumerable<T>
    {
        int Count { get; }
        bool IsReadOnly { get; }

        void Add(T value);
        void Clear();
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
        object[] GetBuffer();
        IEnumerator<T> GetEnumerator();
        bool Remove(T item);
    }
}