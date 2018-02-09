namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using BullOak.Infrastructure.TestHelpers.Application.Stubs;
    using BullOak.Messages;
    using BullOak.Repositories;
    using BullOak.Repositories.Config;
    using BullOak.Repositories.InMemory;
    using BullOak.Test.EndToEnd.Stub.AggregateBased;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    internal class AggregateFixture
    {
        public readonly InMemoryEventSourcedRepository<CinemaAggregateRootId, CinemaAggregateState> CinemaFunctionalRepo;
        public readonly CinemaAggregateRepository CinemaAggregateRepository;
        public CinemaAggregateRootId cinemaId;

        public readonly InMemoryEventSourcedRepository<ViewingId, IViewingState> ViewingFunctionalRepo;
        public readonly ViewingAggregateRepository ViewingAggregateRepository;

        public Guid correlationId;
        public string name;
        public DateTime dateOfViewing;

        public AggregateFixture(string name, object[] appliers = null)
        {
            var configuration = Configuration.Begin()
                .WithDefaultCollection()
                .WithDefaultStateFactory()
                .AlwaysUseThreadSafe()
                .WithNoEventPublisher()
                .WithAnyAppliersFromInstances((IEnumerable<object>) appliers ?? new List<object>())
                .WithAnyAppliersFrom(Assembly.GetAssembly(typeof(CinemaAggregateRepository)))
                .AndNoMoreAppliers()
                .WithNoUpconverters()
                .Build();

            this.name = name;
            correlationId = Guid.NewGuid();
            CinemaFunctionalRepo = new InMemoryEventSourcedRepository<CinemaAggregateRootId, CinemaAggregateState>(configuration);
            ViewingFunctionalRepo = new InMemoryEventSourcedRepository<ViewingId, IViewingState>(configuration);
            CinemaAggregateRepository = new CinemaAggregateRepository(new InMemoryEventStore());
            ViewingAggregateRepository = new ViewingAggregateRepository(new InMemoryEventStore());

            cinemaId = new CinemaAggregateRootId(name);
            dateOfViewing = DateTime.Now.AddDays(3);
        }

        Random r = new Random();
        public void AddCinemaCreationEvent()
        {
            var @event = new CinemaCreated(Guid.NewGuid(), cinemaId, r.Next());

            using (var session = CinemaFunctionalRepo.BeginSessionFor(@event.CinemaId).Result)
            {
                session.AddEvent(@event);
                session.SaveChanges().Wait();
            }

            CinemaAggregateRepository[@event.CinemaId.Name].Add(@event.ToEnvelope(@event.CinemaId)
                .FromAggregate<BullOak.Test.EndToEnd.Stub.AggregateBased.CinemaAggregate.CinemaAggregateRoot>());
        }

        public void AddViewingAndSeatCreationEvents(ViewingId viewingId, int capacity)
        {
            var viewingCreatedEvent = new ViewingCreatedEvent(viewingId, capacity);

            using (var session = ViewingFunctionalRepo.BeginSessionFor(viewingCreatedEvent.ViewingId).Result)
            {
                session.AddEvent(viewingCreatedEvent);
                session.SaveChanges().Wait();
            }

            ViewingAggregateRepository[viewingId.ToString()].Add(viewingCreatedEvent.ToEnvelope(viewingCreatedEvent.ViewingId)
                .FromAggregate<BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate.ViewingAggregateRoot>());

            for (ushort i = 0; i < capacity; i++)
                AddSeatCreatedEvent(viewingCreatedEvent.ViewingId, i);
        }

        private void AddSeatCreatedEvent(ViewingId viewingId, ushort seatNumber)
        {
            var seatCreated = new SeatInViewingInitialized(new SeatId(seatNumber));

            using (var session = ViewingFunctionalRepo.BeginSessionFor(viewingId).Result)
            {
                session.AddEvent(seatCreated);
                session.SaveChanges().Wait();
            }

            ViewingAggregateRepository[viewingId.ToString()].Add(seatCreated.ToEnvelope(new SeatId(seatNumber))
                .FromChildEntity<BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate.SeatsInViewing>()
                .WithParentId(viewingId));
        }

        public void AddSeatReservationEvent(ViewingId viewingId, ushort seatNumber)
        {
            var seatReserved = new SeatReservedEvent(viewingId, new SeatId(seatNumber));

            using (var session = ViewingFunctionalRepo.BeginSessionFor(viewingId).Result)
            {
                session.AddEvent(seatReserved);
                session.SaveChanges().Wait();
            }

            ViewingAggregateRepository[viewingId.ToString()].Add(seatReserved.ToEnvelope(new SeatId(seatNumber))
                .FromChildEntity<BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate.SeatsInViewing>()
                .WithParentId(viewingId));
        }
    }
}