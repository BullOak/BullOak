namespace BullOak.Repositories.Session.CustomLinkedList
{
    internal class Node<T>
    {
        public Node<T> next;
        public T value;

        public Node(T value)
        {
            next = null;
            this.value = value;
        }
    }
}
