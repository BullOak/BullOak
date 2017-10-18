namespace BullOak.Test.EndToEnd.Stub.AggregateBased
{
    using System.Collections.Generic;
    using BullOak.EventStream;
    using BullOak.Infrastructure.TestHelpers.Application.Stubs;
    using BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    public class ViewingAggregateRepository : Repository<ViewingAggregateRoot, ViewingId>
    {
        private readonly InMemoryEventStore eventStore;
        public List<IParcelVisionEventEnvelope> this[string id] => eventStore[id];

        public ViewingAggregateRepository(InMemoryEventStore eventStore) : base(eventStore)
        {
            this.eventStore = eventStore;
        }
    }
}
