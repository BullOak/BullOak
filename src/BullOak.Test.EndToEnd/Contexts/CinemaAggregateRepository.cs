namespace BullOak.Test.EndToEnd.Contexts
{
    using BullOak.Test.EndToEnd.StubSystem.CinemaAggregate;
    using BullOak.EventStream;
    using System.Collections.Generic;
    using BullOak.Infrastructure.TestHelpers.Application.Stubs;

    public class CinemaAggregateRepository : Repository<CinemaAggregateRoot, CinemaAggregateRootId>
    {
        private readonly EventStoreStub eventStore;
        public List<IParcelVisionEventEnvelope> this[string id]
        {
            get { return eventStore[id]; }
        }
        public CinemaAggregateRepository(EventStoreStub eventStore) : base(eventStore)
        {
            this.eventStore = eventStore;
        }
    }
}
