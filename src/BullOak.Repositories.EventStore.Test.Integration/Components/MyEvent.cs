namespace BullOak.Repositories.EventStore.Test.Integration.Components
{
    using System;

    public class MyEvent : IMyEvent
    {
        public Guid Id { get; set; }
        public int Value { get; set; }

        public MyEvent(int value)
        {
            Id = Guid.NewGuid();
            Value = value;
        }
    }
}
