namespace BullOak.Repositories.EventStore.Test.Integration.Components
{
    using System;

    public interface IMyEvent
    {
        Guid Id { get; set; }
        int Value { get; set; }
    }
}