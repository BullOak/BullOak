namespace BullOak.Messages
{
    using System;

    public abstract class ParcelVisionEvent : ParcelVisionMessage, IParcelVisionEvent
    {
        public ParcelVisionEvent(Guid correlationId) : base(correlationId)
        {
        }
    }
}