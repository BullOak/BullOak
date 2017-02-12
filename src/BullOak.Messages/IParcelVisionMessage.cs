namespace BullOak.Messages
{
    using System;

    public interface IParcelVisionMessage
    {
        Guid Id { get; }

        DateTime TimeStamp { get; }

        Guid CorrelationId { get; }
    }
}
