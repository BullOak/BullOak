using System;
using System.Collections.Generic;
namespace BullOak.Application.Test.Unit.Aggregate.Events
{ 
    using System;
    using BullOak.Messages;

    public class AggregateUpdated : ParcelVisionEvent
    {
        public AggregateRootTestId AggregateSutId { get; set; }

        public string Name { get; set; }

        public AggregateUpdated(Guid correlationId, AggregateRootTestId aggregateSutId, string name)
            : base(correlationId)
        {
            AggregateSutId = aggregateSutId;
            Name = name;
        }
    }
}
