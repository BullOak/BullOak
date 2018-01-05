namespace BullOak.Test.EndToEnd.StepDefinitions
{
    using System;
    using System.Linq;
    using BullOak.Test.EndToEnd.Specifications;
    using BullOak.Test.EndToEnd.Stub.AggregateBased;
    using BullOak.Test.EndToEnd.Stub.AggregateBased.CinemaAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;
    using TechTalk.SpecFlow;
    using FluentAssertions;

    [Binding]
    [Scope(Feature = "AggregateBasedES")]
    public class AggregateBasedESStepDefinitions
    {
        private ScenarioContext SC { get; }
        private readonly CinemaAggregateRepository cinemaRepo;

        private CinemaAggregateRoot CinemaAggregateRoot
        {
            get { return (CinemaAggregateRoot)SC["SimpleSpecStepDefinitions.CinemaAggregateRoot"]; }
            set { SC["SimpleSpecStepDefinitions.CinemaAggregateRoot"] = value; }
        }
        private string CinemaName
        {
            get { return (string)SC["SimpleSpecStepDefinitions.CinemaName"]; }
            set { SC["SimpleSpecStepDefinitions.CinemaName"] = value; }
        }

        public AggregateBasedESStepDefinitions(ScenarioContext sc, CinemaAggregateRepository cinemaRepo)
        {
            this.cinemaRepo = cinemaRepo;
            SC = sc;
        }

        [Given(@"I creare a cinema named ""(.*)"" with (.*) seats")]
        public void GivenICreareACinemaNamedWithSeats(string name, int numberOfSeats)
        {
            CinemaAggregateRoot = new CinemaAggregateRoot(Guid.NewGuid(), numberOfSeats, name);
            CinemaName = name;
        }

        [Given(@"the ""(.*)"" cinema with (.*) seats")]
        public void GivenITheCinemaWithSeats(string cinemaName, int numberOfSeats)
        {
            var aggregate = new CinemaAggregateRoot(Guid.NewGuid(), numberOfSeats, cinemaName);
            cinemaRepo.Save(aggregate).Wait();
        }

        [When(@"I load the ""(.*)"" cinema from the repository")]
        public void WhenILoadTheCinemaFromTheRepository(string cinemaName)
            => CinemaAggregateRoot = cinemaRepo.Load(new CinemaAggregateRootId(cinemaName)).Result;

        [When(@"I save the cinema")]
        public void WhenISaveTheCinema()
            => cinemaRepo.Save(CinemaAggregateRoot).Wait();

        [Then("the cinema I get should not be null")]
        public void ThenTheCinemaShouldNotBeNull()
            => CinemaAggregateRoot.Should().NotBeNull();

        [Then(@"a CinemaCreatedEvent should exist")]
        public void ThenACinemaCreatedEventShouldExist()
        {
            var events = cinemaRepo[CinemaName].Select(x=> x.Event).ToList();

            events.Should().NotBeNullOrEmpty();
            events.Count.Should().Be(1);
            events[0].Should().BeOfType<CinemaCreated>();
        }

        [Then(@"the cinema aggregate should have seats set to (.*)")]
        public void ThenItShouldHaveSeatsSetTo(int numberOfSeats)
            => CinemaAggregateRoot.NumberOfSeats.Should().Be(numberOfSeats);

        [Then(@"the cinema aggregate should have a cinema name of ""(.*)""")]
        public void ThenItShouldHaveACinemaNameOf(string name)
            => CinemaAggregateRoot.Id.Name.Should().Be(name);

        [Then(@"the cinema creation event should have seats set to (.*)")]
        public void ThenTheCinemaCreationEventShouldHaveSeatsSetTo(int numberOfSeats)
            => (cinemaRepo[CinemaName][0].Event as CinemaCreated).Capacity.Should().Be(numberOfSeats);

        [Then(@"the cinema creation event should have a cinema name of ""(.*)""")]
        public void ThenTheCinemaCreationEventShouldHaveACinemaNameOf(string name)
            => (cinemaRepo[CinemaName][0].Event as CinemaCreated).Id.Name.Should().Be(name);
    }
}