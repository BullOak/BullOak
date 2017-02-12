namespace BullOak.Messages
{
    using System;

    public class ParcelVisionMessage : IParcelVisionMessage
    {
        public Guid Id { get; } = Guid.NewGuid();

        public DateTime TimeStamp { get; } = DateTime.UtcNow;

        public Guid CorrelationId { get; set; }

        public ParcelVisionMessage(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
