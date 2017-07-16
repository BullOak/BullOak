namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate
{
    using System;

    public class ViewingAggregateRoot
    {
        private AggregateBased.ViewingAggregate.SeatsInViewing[] Seats { get; set; }

        public ViewingAggregateRoot(int numberOfSeats, DateTime timeOfViewing, string movieName)
        {
            throw new NotImplementedException();
        }
    }
}
