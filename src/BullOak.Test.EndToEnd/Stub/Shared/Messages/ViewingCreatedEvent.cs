namespace BullOak.Test.EndToEnd.Stub.Shared.Messages
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public class ViewingCreatedEvent : BullOakEvent
    {
        public ViewingId ViewingId { get; }
        public int Seats { get; }

        public ViewingCreatedEvent(ViewingId viewingAggregateViewingId, int numberOfSeats)
            :base(Guid.NewGuid())
        {
            ViewingId = viewingAggregateViewingId;
            Seats = numberOfSeats;
        }
    }
}
