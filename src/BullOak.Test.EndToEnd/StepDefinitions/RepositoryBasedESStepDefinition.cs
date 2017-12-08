namespace BullOak.Test.EndToEnd.StepDefinitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BullOak.Repositories;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;
    using FluentAssertions;
    using TechTalk.SpecFlow;

    using CinemaRepository = BullOak.Repositories.InMemory.InMemoryEventSourcedRepository<Stub.RepositoryBased.CinemaAggregate.CinemaAggregateState, Stub.Shared.Ids.CinemaAggregateRootId>;

    [Binding]
    [Scope(Feature = "RepositoryBasedES")]
    internal class RepositoryBasedESStepDefinition
    {
        private readonly ScenarioContext scenarioContext;

        private string CinemaName
        {
            get => (string) scenarioContext["SimpleSpecStepDefinitions.CinemaName"];
            set => scenarioContext["SimpleSpecStepDefinitions.CinemaName"] = value;
        }

        private CinemaRepository CinemaRepo
        {
            get => (CinemaRepository)scenarioContext[nameof(CinemaRepo)];
            set => scenarioContext[nameof(CinemaRepo)] = value;
        }

        private CinemaAggregateState CinemaState
        {
            get => (CinemaAggregateState) scenarioContext[nameof(CinemaState)];
            set => scenarioContext[nameof(CinemaState)] = value;
        }

        private CinemaAggregateRootId CinemaId
        {
            get => (CinemaAggregateRootId) scenarioContext[nameof(CinemaId)];
            set => scenarioContext[nameof(CinemaId)] = value;
        }

        private List<object> CinemaOperationEvents
        {
            get => (List<object>) scenarioContext[nameof(CinemaOperationEvents)];
            set => scenarioContext[nameof(CinemaOperationEvents)] = value;
        }

        public RepositoryBasedESStepDefinition(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            CinemaOperationEvents = new List<object>();
            CinemaRepo = new CinemaRepository(StubDI.GetCreator());
        }

        [Given(@"a cinema creation event with the name ""(.*)"" and (.*) seats in the event stream")]
        public void GivenACinemaCreationEventWithTheNameAndSeatsInTheEventStream(string name, int capacity)
        {
            CinemaId = new CinemaAggregateRootId(name);

            using (var session = CinemaRepo.Load(CinemaId))
            {
                var @event = new CinemaAggregateRoot()
                    .Create(Guid.NewGuid(), capacity, name);

                session.AddToStream(@event);

                session.SaveChanges().Wait();
            }
        }

        [Given(@"I creare a cinema named ""(.*)"" with (.*) seats")]
        public void GivenICreareACinemaNamedWithSeats(string name, int capacity)
        {
            var @event = new CinemaAggregateRoot()
                .Create(Guid.NewGuid(), capacity, name);

            CinemaId = @event.Id;
            CinemaOperationEvents.Add(@event);
        }

        [When(@"I load the ""(.*)"" cinema from the repository")]
        public void WhenILoadTheCinemaFromTheRepository(string name)
        {
            var id = new CinemaAggregateRootId(name);

            using (var session = CinemaRepo.Load(id))
            {
                CinemaState = session.GetCurrentState();
            }
        }

        [When(@"I save the cinema")]
        public void WhenISaveTheCinema()
        {
            using (var session = CinemaRepo.Load(CinemaId))
            {
                session.AddToStream(CinemaOperationEvents.ToArray());
                CinemaOperationEvents.Clear();

                session.SaveChanges();
            }
        }

        [Then(@"a CinemaCreatedEvent should exist")]
        public void ThenACinemaCreatedEventShouldExist()
        {
            using (var session = CinemaRepo.Load(CinemaId))
            {
                session.EventStream.First().Should().NotBeNull();
                session.EventStream.First().Should().BeAssignableTo<CinemaCreated>();
            }
        }

        [Then(@"the cinema creation event should have seats set to (.*)")]
        public void ThenTheCinemaCreationEventShouldHaveSeatsSetTo(int capacity)
        {
            using (var session = CinemaRepo.Load(CinemaId))
            {
                session.EventStream.First().Should().NotBeNull();
                session.EventStream.First().As<CinemaCreated>()
                    .Capacity.Should().Be(capacity);
            }
        }

        [Then(@"the cinema creation event should have a cinema name of ""(.*)""")]
        public void ThenTheCinemaCreationEventShouldHaveACinemaNameOf(string name)
        {
            using (var session = CinemaRepo.Load(CinemaId))
            {
                session.EventStream.First().Should().NotBeNull();
                session.EventStream.First().As<CinemaCreated>()
                    .Id.Name.Should().Be(name);
            }
        }

        [Then(@"the cinema I get should not be null")]
        public void ThenTheCinemaIGetShouldNotBeNull()
        {
            using (var session = CinemaRepo.Load(CinemaId))
            {
                session.GetCurrentState().Should().NotBeNull();
            }
        }

        [Then(@"the cinema aggregate state should have seats set to (.*)")]
        public void ThenTheCinemaAggregateShouldHaveSeatsSetTo(int capacity)
        {
            using (var session = CinemaRepo.Load(CinemaId))
            {
                session.GetCurrentState().NumberOfSeats.Should().Be(capacity);
            }
        }

        [Then(@"the cinema aggregate state should have a cinema name of ""(.*)""")]
        public void ThenTheCinemaAggregateShouldHaveACinemaNameOf(string name)
        {
            using (var session = CinemaRepo.Load(CinemaId))
            {
                session.GetCurrentState().Id.Name.Should().Be(name);
            }
        }
    }
}
