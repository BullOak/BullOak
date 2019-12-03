namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using BullOak.Repositories;
    using BullOak.Repositories.Config;
    using BullOak.Repositories.InMemory;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    internal class AggregateFixture
    {
        public readonly InMemoryEventSourcedRepository<CinemaAggregateRootId, CinemaAggregateState> CinemaFunctionalRepo;
        public CinemaAggregateRootId cinemaId;

        public readonly InMemoryEventSourcedRepository<ViewingId, IViewingState> ViewingFunctionalRepo;

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
                .WithAnyAppliersFrom(Assembly.GetAssembly(typeof(CinemaAggregateState)))
                .AndNoMoreAppliers()
                .WithNoUpconverters()
                .Build();

            this.name = name;
            correlationId = Guid.NewGuid();
            CinemaFunctionalRepo = new InMemoryEventSourcedRepository<CinemaAggregateRootId, CinemaAggregateState>(configuration);
            ViewingFunctionalRepo = new InMemoryEventSourcedRepository<ViewingId, IViewingState>(configuration);

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
        }

        public void AddViewingAndSeatCreationEvents(ViewingId viewingId, int capacity)
        {
            var viewingCreatedEvent = new ViewingCreatedEvent(viewingId, capacity);

            using (var session = ViewingFunctionalRepo.BeginSessionFor(viewingCreatedEvent.ViewingId).Result)
            {
                session.AddEvent(viewingCreatedEvent);
                session.SaveChanges().Wait();
            }

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
        }

        public void AddSeatReservationEvent(ViewingId viewingId, ushort seatNumber)
        {
            var seatReserved = new SeatReservedEvent(viewingId, new SeatId(seatNumber));

            using (var session = ViewingFunctionalRepo.BeginSessionFor(viewingId).Result)
            {
                session.AddEvent(seatReserved);
                session.SaveChanges().Wait();
            }
        }
    }
}
