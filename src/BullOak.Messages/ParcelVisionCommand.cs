namespace BullOak.Messages
{
    using System;

    public abstract class ParcelVisionCommand : ParcelVisionMessage, IParcelVisionCommand
    {
        public ParcelVisionCommand(Guid correlationId) : base(correlationId)
        {
        }
}
}
