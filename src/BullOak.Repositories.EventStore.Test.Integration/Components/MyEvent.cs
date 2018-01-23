namespace BullOak.Repositories.EventStore.Test.Integration.Components
{
    using System;

    public class MyEvent
    {
        public Guid Id { get; private set; }
        public int Value { get; private set; }

        public MyEvent(int order)
        {
            Id = Guid.NewGuid();
            Value = order;
        }
    }

}
