namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate
{
    using System;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public interface IViewingState
    {
        ViewingId ViewingId { get; set; }
        Seats[] Seats { get; set; }
    }

    public class Seats
    {
        public int Id { get; set; }
        public bool IsReserved { get; set; }
    }
}
