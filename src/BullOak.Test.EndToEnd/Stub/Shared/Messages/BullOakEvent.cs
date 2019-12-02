namespace BullOak.Test.EndToEnd.Stub.Shared.Messages
{
    using System;

    public class BullOakEvent
    {
        public Guid CorrelationId { get; set; }
        private DateTime CreatedOn { get; set; }

        public BullOakEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
            CreatedOn = DateTime.UtcNow;
        }
    }
}
