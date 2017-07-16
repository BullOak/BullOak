namespace BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate
{
    using System;

    public class ViewingAggregateRoot
    {
        private SeatsInViewing[] Seats { get; set; }

        public ViewingAggregateRoot(int numberOfSeats, DateTime timeOfViewing, string movieName)
        {
            throw new NotImplementedException();
        }
    }
}
