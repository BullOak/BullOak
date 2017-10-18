namespace BullOak.Test.EndToEnd.StepDefinitions
{
    using System;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;
    using FluentAssertions;
    using TechTalk.SpecFlow;

    using ViewingRepository = BullOak.Repositories.InMemory.InMemoryEventSourcedRepository<Stub.RepositoryBased.ViewingAggregate.ViewingState, Stub.Shared.Ids.ViewingId>;

    [Binding]
    [Scope(Feature = "RepositoryBasedES")]
    public class RepositoryBasedChildEntityESSteps
    {
        private ViewingAggregateRoot ViewingAggregate { get; } = new ViewingAggregateRoot();

        private ViewingRepository ViewingRepository { get; set; }
        private readonly ScenarioContext scenarioContext;

        private ViewingId ViewingId
        {
            get => (ViewingId) scenarioContext[nameof(ViewingId)];
            set => scenarioContext[nameof(ViewingId)] = value;
        }

        private object Event { get; set; }

        public RepositoryBasedChildEntityESSteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            ViewingRepository = new ViewingRepository(StubDI.GetCreator());
        }

        [Given(@"a viewing for ""(.*)"" with (.*) seats in (.*) hours from now")]
        public void GivenAViewingForWithSeatsInDaysFromNow(string movie, int seats, int hoursUntilViewing)
        {
            var cinemaId= new CinemaAggregateRootId("CinemaId");
            ViewingId = new ViewingId(movie, DateTime.Now.AddHours(hoursUntilViewing), cinemaId);
            using (var session = ViewingRepository.Load(ViewingId))
            {
                var creationEvent = ViewingAggregate.CreateViewing(cinemaId, movie, ViewingId.ShowingDate, seats);

                session.AddToStream(creationEvent);
                session.SaveChanges();
            }
        }

        [When(@"I try to reserve seat (.*)")]
        public void WhenITryToReserveSeat(int seatToReserve)
        {
            using (var session = ViewingRepository.Load(ViewingId))
            {
                var state = session.State;
                Event = ViewingAggregate.ReserveSeat(state, seatToReserve);

                session.AddToStream(Event);
                session.SaveChanges();
            }
        }

        [Then(@"I should get a seat reserved event for seat (.*)")]
        public void ThenIShouldGetASeatReservedEvent(int seatToReserve)
        {
            Event.Should().NotBeNull();
            Event.Should().BeOfType<SeatReservedEvent>();
            Event.As<SeatReservedEvent>().ViewingId.ShowingDate.Should().Be(ViewingId.ShowingDate);
            Event.As<SeatReservedEvent>().ViewingId.CinemaId.Name.Should().Be(ViewingId.CinemaId.Name);
            Event.As<SeatReservedEvent>().ViewingId.MovieName.Should().Be(ViewingId.MovieName);
            Event.As<SeatReservedEvent>().IdOfSeatToReserve.Id.Should().Be((ushort)seatToReserve);
        }
    }
}
