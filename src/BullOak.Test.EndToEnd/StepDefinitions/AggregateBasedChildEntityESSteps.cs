namespace BullOak.Test.EndToEnd.StepDefinitions
{
    using System;
    using System.Linq;
    using BullOak.Test.EndToEnd.Stub.AggregateBased;
    using BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;
    using FluentAssertions;
    using TechTalk.SpecFlow;
    using Xunit;

    [Binding]
    [Scope(Feature = "AggregateBasedES")]
    public class AggregateBasedChildEntityESSteps
    {
        private ViewingAggregateRoot ViewingAggregate { get; set; }

        private ViewingAggregateRepository ViewingRepository { get; set; }
        private readonly ScenarioContext scenarioContext;

        private Exception Exception;

        private ViewingId ViewingId
        {
            get => (ViewingId) scenarioContext[nameof(ViewingId)];
            set => scenarioContext[nameof(ViewingId)] = value;
        }

        public AggregateBasedChildEntityESSteps(ScenarioContext scenarioContext, ViewingAggregateRepository aggregateRepo)
        {
            this.scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            ViewingRepository = aggregateRepo;
        }

        [Given(@"a viewing for ""(.*)"" with (.*) seats in (.*) hours from now")]
        public void GivenAViewingForWithSeatsInDaysFromNow(string movie, int seats, int hoursUntilViewing)
        {
            var cinemaId= new CinemaAggregateRootId("CinemaId");
            ViewingId = new ViewingId(movie, DateTime.Now.AddHours(hoursUntilViewing), cinemaId);
            ViewingAggregate = new ViewingAggregateRoot(seats, ViewingId.ShowingDate, ViewingId.MovieName, ViewingId.CinemaId);
        }

        [When(@"I save the viewing aggregate")]
        public void WhenISaveTheViewingAggregate()
        {
            ViewingRepository.Save(ViewingAggregate).Wait();
        }


        [When(@"I try to reserve seat (.*)")]
        public void WhenITryToReserveSeat(int seatToReserve)
        {
            Exception = Record.Exception(() => ViewingAggregate.ReserveSeat(seatToReserve));
        }

        [Then(@"I should have (.*) seat created events")]
        public void ThenIShouldHaveSeatCreatedEvents(int count)
        {
            ViewingRepository[ViewingId.ToString()].Count(x => x.Event is SeatInViewingInitialized).Should().Be(count);
        }

        [Then(@"I should get a seat reserved event for seat (.*)")]
        public void ThenIShouldGetASeatReservedEvent(int seatToReserve)
        {
            ViewingRepository[ViewingId.ToString()].Any(x => x.Event is SeatReservedEvent).Should().BeTrue();
            var @event = ViewingRepository[ViewingId.ToString()].First(x => x.Event is SeatReservedEvent).Event;
            @event.Should().NotBeNull();
            @event.Should().BeOfType<SeatReservedEvent>();
            @event.As<SeatReservedEvent>().ViewingId.ShowingDate.Should().Be(ViewingId.ShowingDate);
            @event.As<SeatReservedEvent>().ViewingId.CinemaId.Name.Should().Be(ViewingId.CinemaId.Name);
            @event.As<SeatReservedEvent>().ViewingId.MovieName.Should().Be(ViewingId.MovieName);
            @event.As<SeatReservedEvent>().IdOfSeatToReserve.Id.Should().Be((ushort)seatToReserve);
        }
    }
}
