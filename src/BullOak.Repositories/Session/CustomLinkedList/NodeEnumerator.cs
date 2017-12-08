namespace BullOak.Repositories.Session.CustomLinkedList
{
    using System.Collections;
    using System.Collections.Generic;

    internal class NodeEnumerator<T> : IEnumerator<T>
    {
        public void Dispose() { }

        public bool MoveNext()
        {
            if (currentNode.next == null) return false;

            currentNode = currentNode.next;
            return true;
        }

        public void Reset() => throw new System.NotSupportedException();

        private Node<T> currentNode;
        public T Current => currentNode.value;
        object IEnumerator.Current => Current;

        public NodeEnumerator(Node<T> current, int count)
        {
            currentNode = current;
        }
    }
}