using System;

namespace BullOak.Messages
{
    public abstract class ParcelVisionErrorEvent : ParcelVisionEvent, IParcelVisionErrorEvent
    {
        public string Message { get; set; }

        public Exception Exception { get; set; }

        public ParcelVisionErrorEvent(Guid correlationId, string message, Exception ex = null) : base(correlationId)
        {
            Message = message;
            Exception = ex;
        }
    }
}
