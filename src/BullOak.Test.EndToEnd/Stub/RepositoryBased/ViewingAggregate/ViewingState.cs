namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public class ViewingState
    {
        public ViewingId ViewingId { get; set; }
        public Seats[] Seats { get; set; }
        public DateTime TimeOfShowing => ViewingId.ShowingDate;
    }

    public class Seats
    {
        public int Id { get; set; }
        public bool IsReserved { get; set; }
    }
}
