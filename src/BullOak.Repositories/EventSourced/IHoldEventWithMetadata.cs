namespace BullOak.Repositories.EventSourced
{
    using System;
    using System.Collections.Generic;

    public interface IHoldEventWithMetadata
    {
        Type EventType { get; }
        Dictionary<string, string> Metadata { get; }
        object Event { get; }
    }

    public interface IHoldEventWithMetadata<out TEvent> : IHoldEventWithMetadata
    {
        new TEvent Event { get; }
    }
}
