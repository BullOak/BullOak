namespace BullOak.Test.EndToEnd.Stub.Shared.Messages
{
    using System;
    using BullOak.Messages;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public class ViewingCreatedEvent : ParcelVisionEvent
    {
        public ViewingId Id { get; }
        public int Seats { get; }

        public ViewingCreatedEvent(ViewingId viewingAggregateId, int numberOfSeats)
            :base(Guid.NewGuid())
        {
            Id = viewingAggregateId;
            Seats = numberOfSeats;
        }
    }
}