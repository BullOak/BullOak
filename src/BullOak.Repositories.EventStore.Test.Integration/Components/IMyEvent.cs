using System;

namespace BullOak.Repositories.EventStore.Test.Integration.Components
{
    public interface IMyEvent
    {
        Guid Id { get; set; }
        int Value { get; set; }
    }
}