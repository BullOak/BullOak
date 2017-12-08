namespace BullOak.Repositories.Session.CustomLinkedList
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class LinkedList<T> : ILinkedList<T>
    {
        private Node<T> first;
        private Node<T> last;
        private int count;

        public int Count => count;
        public bool IsReadOnly { get; }
        private SpinLock mylock = new SpinLock(false);

        public void Add(T value)
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            var newNode = new Node<T>(value);

            bool lockTaken = false;

            mylock.Enter(ref lockTaken);
            if (!lockTaken) throw new Exception();

            var original = last;
            last = newNode;
            count++;

            if (original == null)
            {
                first = newNode;
            }
            else
            {
                original.next = newNode;
            }

            mylock.Exit(false);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            bool lockTaken = false;
            mylock.Enter(ref lockTaken);

            first = null;
            last = null;
            count = 0;

            mylock.Exit(false);
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
            var lockTaken = false;
            mylock.Enter(ref lockTaken);
            var buffer = new object[count];
            var node = first;
            mylock.Exit(false);

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