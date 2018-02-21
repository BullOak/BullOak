namespace BullOak.Test.EndToEnd.StepDefinitions
{
    using BullOak.Repositories;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TechTalk.SpecFlow;
    using CinemaRepository = BullOak.Repositories.InMemory.InMemoryEventSourcedRepository<
        Stub.Shared.Ids.CinemaAggregateRootId, Stub.RepositoryBased.CinemaAggregate.CinemaAggregateState>;

    [Binding]
    [Scope(Feature = "RepositoryBasedES")]
    internal class RepositoryBasedESStepDefinition
    {
        private readonly ScenarioContext scenarioContext;

        private string CinemaName
        {
            get => (string)scenarioContext["SimpleSpecStepDefinitions.CinemaName"];
            set => scenarioContext["SimpleSpecStepDefinitions.CinemaName"] = value;
        }

        private CinemaRepository CinemaRepo
        {
            get => (CinemaRepository)scenarioContext[nameof(CinemaRepo)];
            set => scenarioContext[nameof(CinemaRepo)] = value;
        }

        private CinemaAggregateState CinemaState
        {
            get => (CinemaAggregateState)scenarioContext[nameof(CinemaState)];
            set => scenarioContext[nameof(CinemaState)] = value;
        }

        private CinemaAggregateRootId CinemaId
        {
            get => (CinemaAggregateRootId)scenarioContext[nameof(CinemaId)];
            set => scenarioContext[nameof(CinemaId)] = value;
        }

        private List<object> CinemaOperationEvents
        {
            get => (List<object>)scenarioContext[nameof(CinemaOperationEvents)];
            set => scenarioContext[nameof(CinemaOperationEvents)] = value;
        }

        public RepositoryBasedESStepDefinition(ScenarioContext scenarioContext, IHoldAllConfiguration configuration)
        {
            this.scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            CinemaOperationEvents = new List<object>();
            CinemaRepo = new CinemaRepository(configuration);
        }

        [Given(@"a cinema creation event with the name ""(.*)"" and (.*) seats in the event stream")]
        public async Task GivenACinemaCreationEventWithTheNameAndSeatsInTheEventStream(string name, int capacity)
        {
            CinemaId = new CinemaAggregateRootId(name);

            using (var session = await CinemaRepo.BeginSessionFor(CinemaId))
            {
                var @event = new CinemaAggregateRoot()
                    .Create(Guid.NewGuid(), capacity, name);

                session.AddEvent(@event);

                await session.SaveChanges();
            }
        }

        [Given(@"I creare a cinema named ""(.*)"" with (.*) seats")]
        public void GivenICreareACinemaNamedWithSeats(string name, int capacity)
        {
            var @event = new CinemaAggregateRoot()
                .Create(Guid.NewGuid(), capacity, name);

            CinemaId = @event.CinemaId;
            CinemaOperationEvents.Add(@event);
        }

        [When(@"I load the ""(.*)"" cinema from the repository")]
        public async Task WhenILoadTheCinemaFromTheRepository(string name)
        {
            var id = new CinemaAggregateRootId(name);

            using (var session = await CinemaRepo.BeginSessionFor(id))
            {
                CinemaState = session.GetCurrentState();
            }
        }

        [When(@"I save the cinema")]
        public async Task WhenISaveTheCinema()
        {
            using (var session = await CinemaRepo.BeginSessionFor(CinemaId))
            {
                session.AddEvent(CinemaOperationEvents.ToArray());
                CinemaOperationEvents.Clear();

                await session.SaveChanges();
            }
        }

        [Then(@"a CinemaCreatedEvent should exist")]
        public void ThenACinemaCreatedEventShouldExist()
        {
            CinemaRepo[CinemaId].FirstOrDefault().instance.Should().NotBeNull();
            CinemaRepo[CinemaId].FirstOrDefault().instance.Should().BeAssignableTo<CinemaCreated>();
        }

        [Then(@"the cinema creation event should have seats set to (.*)")]
        public void ThenTheCinemaCreationEventShouldHaveSeatsSetTo(int capacity)
        {
            CinemaRepo[CinemaId].FirstOrDefault().Should().NotBeNull();
            CinemaRepo[CinemaId].FirstOrDefault().instance.As<CinemaCreated>()
                .Capacity.Should().Be(capacity);
        }

        [Then(@"the cinema creation event should have a cinema name of ""(.*)""")]
        public void ThenTheCinemaCreationEventShouldHaveACinemaNameOf(string name)
        {
            CinemaRepo[CinemaId].FirstOrDefault().instance.Should().NotBeNull();
            CinemaRepo[CinemaId].FirstOrDefault().instance.As<CinemaCreated>()
                .CinemaId.Name.Should().Be(name);
        }

        [Then(@"the cinema I get should not be null")]
        public async Task ThenTheCinemaIGetShouldNotBeNull()
        {
            using (var session = await CinemaRepo.BeginSessionFor(CinemaId))
            {
                session.GetCurrentState().Should().NotBeNull();
            }
        }

        [Then(@"the cinema aggregate state should have seats set to (.*)")]
        public async Task ThenTheCinemaAggregateShouldHaveSeatsSetTo(int capacity)
        {
            using (var session = await CinemaRepo.BeginSessionFor(CinemaId))
            {
                session.GetCurrentState().NumberOfSeats.Should().Be(capacity);
            }
        }

        [Then(@"the cinema aggregate state should have a cinema name of ""(.*)""")]
        public async Task ThenTheCinemaAggregateShouldHaveACinemaNameOf(string name)
        {
            using (var session = await CinemaRepo.BeginSessionFor(CinemaId))
            {
                session.GetCurrentState().Id.Name.Should().Be(name);
            }
        }
    }
}
