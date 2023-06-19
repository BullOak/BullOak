using BullOak.Repositories.Session;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BullOak.Repositories.Test.Unit.Session
{
    public class SessionSpecs
    {
        private readonly InMemory.InMemoryEventSourcedRepository<int, TestState> repository;
        private readonly int id;

        public SessionSpecs()
        {
            id = Random.Shared.Next();
            repository = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .WithEventApplier(new CountChangedApplier())
                .AndLoadedAsynchronously()
                .GetNewSUT<int>() as InMemory.InMemoryEventSourcedRepository<int, TestState>;
        }

        private async Task<InMemory.InMemoryEventStoreSession<TestState, int>> Arrange()
        {
            //Arrange
            repository.Should().NotBeNull();
            var session = await repository.BeginSessionFor(id) as InMemory.InMemoryEventStoreSession<TestState, int>;
            session.Should().NotBeNull();
            return session;
        }

        private CountChangedClassEvent GetNextEvent()
        {
            return new CountChangedClassEvent()
            {
                NewCount = Random.Shared.Next()
            };
        }


        [Fact]
        public async Task Session_SaveChangesTwice_ShouldNotThrowConcurrencyException()
        {
            //Arrange
            var sut = await Arrange();
            sut.AddEvent(GetNextEvent());

            //Act
            await sut.SaveChanges();
            var exception = await Record.ExceptionAsync(() => sut.SaveChanges());

            //Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task Session_SaveChangesTwice_OnlyStoreEventsOnce()
        {
            //Arrange
            var sut = await Arrange();
            sut.AddEvent(GetNextEvent());

            //Act
            await sut.SaveChanges();
            await sut.SaveChanges();

            //Assert
            repository[id].Length.Should().Be(1);
        }

        [Fact]
        public async Task Session_SaveChangesThenAddEventsThenSaveAgain_ShouldSaveAllEventsOnce()
        {
            //Arrange
            var sut = await Arrange();
            sut.AddEvent(GetNextEvent());
            var lastEvent = GetNextEvent();

            //Act
            await sut.SaveChanges();
            sut.AddEvent(lastEvent);
            await sut.SaveChanges();

            //Assert
            repository[id].Length.Should().Be(2);
        }

        [Fact]
        public async Task Session_SaveChangesTwice_ShouldUpdateStateToLastEvent()
        {
            //Arrange
            var sut = await Arrange();
            sut.AddEvent(GetNextEvent());
            var lastEvent = GetNextEvent();

            //Act
            await sut.SaveChanges();
            sut.AddEvent(lastEvent);
            await sut.SaveChanges();

            //Assert
            sut.GetCurrentState().Count.Should().Be(lastEvent.NewCount);
        }
    }
}
