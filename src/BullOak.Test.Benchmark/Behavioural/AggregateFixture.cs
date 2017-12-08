namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using BullOak.Infrastructure.TestHelpers.Application.Stubs;
    using BullOak.Messages;
    using BullOak.Repositories;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.InMemory;
    using BullOak.Test.EndToEnd.Stub.AggregateBased;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    internal class AggregateFixture
    {
        public readonly InMemoryEventSourcedRepository<CinemaAggregateState, CinemaAggregateRootId> CinemaFunctionalRepo;
        public readonly CinemaAggregateRepository CinemaAggregateRepository;
        public CinemaAggregateRootId cinemaId;

        public readonly InMemoryEventSourcedRepository<ViewingState, ViewingId> ViewingFunctionalRepo;
        public readonly ViewingAggregateRepository ViewingAggregateRepository;

        public Guid correlationId;
        public string name;
        public DateTime dateOfViewing;

        public AggregateFixture(string name, ICreateEventAppliers fakeDi = null)
        {
            this.name = name;
            correlationId = Guid.NewGuid();
            CinemaFunctionalRepo = new InMemoryEventSourcedRepository<CinemaAggregateState, CinemaAggregateRootId>(fakeDi ?? StubDI.GetCreator());
            ViewingFunctionalRepo = new InMemoryEventSourcedRepository<ViewingState, ViewingId>(fakeDi ?? StubDI.GetCreator());
            CinemaAggregateRepository = new CinemaAggregateRepository(new InMemoryEventStore());
            ViewingAggregateRepository = new ViewingAggregateRepository(new InMemoryEventStore());

            cinemaId = new CinemaAggregateRootId(name);
            dateOfViewing = DateTime.Now.AddDays(3);
        }

        public static ICreateEventAppliers GetAppliersWith<TState>(int totalAppliers, IApplyEvents<TState> custom,
            int customIndex)
        {
            var container = new EventApplierContainer();

            for (int i = 0; i < totalAppliers; i++)
            {
                container.Register(i == customIndex ? custom : new FakeApplier<TState>());
            }
            return container.Build();
        }

        private class FakeApplier<TState> : IApplyEvents<TState>
        {
            public bool CanApplyEvent(object @event) => false;
            public TState Apply(TState state, object @event) => state;
        }

        public void AddCinemaCreationEvent()
        {
            var @event = new CinemaCreated(Guid.NewGuid(), cinemaId, 2);

            using (var session = CinemaFunctionalRepo.Load(@event.Id))
            {
                session.AddToStream(@event);
                session.SaveChanges();
            }

            CinemaAggregateRepository[@event.Id.Name].Add(@event.ToEnvelope(@event.Id)
                .FromAggregate<BullOak.Test.EndToEnd.Stub.AggregateBased.CinemaAggregate.CinemaAggregateRoot>());
        }

        public void AddViewingAndSeatCreatiuonEvents(ViewingId viewingId, int capacity)
        {
            var viewingCreatedEvent = new ViewingCreatedEvent(viewingId, capacity);

            using (var session = ViewingFunctionalRepo.Load(viewingCreatedEvent.Id))
            {
                session.AddToStream(viewingCreatedEvent);
                session.SaveChanges();
            }

            ViewingAggregateRepository[viewingId.ToString()].Add(viewingCreatedEvent.ToEnvelope(viewingCreatedEvent.Id)
                .FromAggregate<BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate.ViewingAggregateRoot>());

            for (ushort i = 0; i < capacity; i++)
                AddSeatCreatedEvent(viewingCreatedEvent.Id, i);
        }

        private void AddSeatCreatedEvent(ViewingId viewingId, ushort seatNumber)
        {
            var seatCreated = new SeatInViewingInitialized(new SeatId(seatNumber));

            using (var session = ViewingFunctionalRepo.Load(viewingId))
            {
                session.AddToStream(seatCreated);
                session.SaveChanges();
            }

            ViewingAggregateRepository[viewingId.ToString()].Add(seatCreated.ToEnvelope(new SeatId(seatNumber))
                .FromChildEntity<BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate.SeatsInViewing>()
                .WithParentId(viewingId));
        }

        public void AddSeatReservationEvent(ViewingId viewingId, ushort seatNumber)
        {
            var seatReserved = new SeatReservedEvent(viewingId, new SeatId(seatNumber));

            using (var session = ViewingFunctionalRepo.Load(viewingId))
            {
                session.AddToStream(seatReserved);
                session.SaveChanges();
            }

            ViewingAggregateRepository[viewingId.ToString()].Add(seatReserved.ToEnvelope(new SeatId(seatNumber))
                .FromChildEntity<BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate.SeatsInViewing>()
                .WithParentId(viewingId));
        }
    }
}