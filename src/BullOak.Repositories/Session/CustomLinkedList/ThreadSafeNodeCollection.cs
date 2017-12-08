namespace BullOak.Repositories.Session.CustomLinkedList
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class ThreadSafeLinkedList<T> : ICollection<T>, IReadOnlyCollection<T>, IEnumerable<T>
    {
        private Node<T> first;
        private Node<T> last;
        private int count;

        public int Count => count;
        public bool IsReadOnly { get; }

        public void Add(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var previousLast = last;
            last = new Node<T>(value);
            if (previousLast == null) first = last;
            else previousLast.next = last;
            count++;
        }

        public bool Remove(T item)
            => throw new NotSupportedException();

        public void Clear()
        {
            first = null;
            last = null;
            count = 0;
        }

        public bool Contains(T item) => ((IEnumerable<T>) this).Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            var node = first;
            for (int i = arrayIndex; i < array.Length && node != null; i++)
            {
                array[i] = node.value;
                node = node.next;
            }
        }

        public object[] GetBuffer()
        {
            var buffer = new object[count];
            var node = first;

            for (int i = 0; i < buffer.Length && node != null; i++)
            {
                buffer[i] = node.value;
                node = node.next;
            }

            return buffer;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new NodeEnumerator<T>(new Node<T>(default(T)) { next = first }, count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}