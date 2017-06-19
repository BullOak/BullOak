namespace BullOak.Test.EndToEnd.Contexts
{
    using BullOak.Test.EndToEnd.StubSystem.CinemaAggregate;
    using BullOak.EventStream;
    using System.Collections.Generic;
    using BullOak.Infrastructure.TestHelpers.Application.Stubs;

    public class CinemaAggregateRepository : Repository<CinemaAggregateRoot, CinemaAggregateRootId>
    {
        private readonly InMemoryEventStore eventStore;
        public List<IParcelVisionEventEnvelope> this[string id]
        {
            get { return eventStore[id]; }
        }
        public CinemaAggregateRepository(InMemoryEventStore eventStore) : base(eventStore)
        {
            this.eventStore = eventStore;
        }
    }
}
