namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;

    public class MyEvent
    {
        public Guid Id { get; private set; }
        public int Order { get; private set; }

        public MyEvent(int order)
        {
            Id = Guid.NewGuid();
            Order = order;
        }
    }
}