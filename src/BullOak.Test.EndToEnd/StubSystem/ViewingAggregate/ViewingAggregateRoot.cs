namespace BullOak.Test.EndToEnd.StubSystem.ViewingAggregate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ViewingAggregateRoot
    {
        private SeatsInViewing[] Seats { get; set; }

        public ViewingAggregateRoot(int numberOfSeats, DateTime timeOfViewing, string movieName)
        {
            throw new NotImplementedException(); 
        }


    }
}
