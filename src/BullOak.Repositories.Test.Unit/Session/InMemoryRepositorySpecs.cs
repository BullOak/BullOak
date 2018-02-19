namespace BullOak.Repositories.Test.Unit.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.InMemory;
    using BullOak.Repositories.Session;
    using BullOak.Repositories.Upconverting;
    using FluentAssertions;
    using Xunit;

    public static class InMemoryEventSourcedRepositoryTestExtensions
    {
        public static InMemoryEventSourcedRepository<TId, TState> WithEventInStream<TState, TId>(
            this InMemoryEventSourcedRepository<TId, TState> sut,
            object @event,
            TId streamId)
        {
            var currentEvents = sut[streamId]?.ToList() ?? new List<object>();
            currentEvents.Add(@event);
            sut[streamId] = currentEvents.ToArray();
            return sut;
        }
    }

    public class InMemoryRepositorySpecs
    {
        internal class TestState
        {
            public int Count { get; set; }
            public string Name { get; set; }
        }

        internal class CountChangedClassEvent
        {
            public int NewCount { get; set; }
        }

        internal class CountChangedApplier : IApplyEventsToStates
        {
            public IEnumerable<Type> SupportedStateTypes => new[] {typeof(TestState)};

            public TestState Apply(TestState state, CountChangedClassEvent @event)
            {
                state.Count = @event.NewCount;
                return state;
            }

            public object Apply(Type stateType, object state, ItemWithType[] events)
                => events.Aggregate(state, (s, e) => ApplyEvent(stateType, s, e));

            public object Apply(Type stateType, object state, IEnumerable<ItemWithType> events)
                => events.Aggregate(state, (s, e) => ApplyEvent(stateType, s, e));

            public object ApplyEvent(Type stateType, object state, ItemWithType @event)
            {
                if (@event.instance is CountChangedClassEvent countEvent)
                {
                    return Apply(state as TestState, countEvent);
                }

                throw new NotSupportedException();
            }
        }

        [Fact]
        public async Task BeginSession_WithNewIdAndThrowIfNotExist_ShouldThrowException()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();

            //Act
            var exception = await Record.ExceptionAsync(() => sut.BeginSessionFor(123, throwIfNotExists: true));

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<StreamNotFoundException>();
        }

        [Fact]
        public async Task BeginSession_WithNewId_ShouldReturnSession()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();

            //Act
            var session = await sut.BeginSessionFor(123);

            //Assert
            session.Should().NotBeNull();
            session.Should().BeAssignableTo<IManageSessionOf<TestState>>();
        }

        [Fact]
        public async Task SaveEvents_WithOneNewEvent_ShouldAddEventInStore()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();
            var @event = new object();
            var id = 42;

            using (var session = await sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent(@event);
                await session.SaveChanges();
            }

            //Assert
            sut[id].Should().NotBeNull();
            sut[id].Length.Should().Be(1);
            sut[id][0].Should().Be(@event);
        }

        [Fact]
        public async Task SaveEvents_WithOneNewInitializedEvent_ShouldAddEventInStore()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .WithEventApplier(new CountChangedApplier())
                .GetNewSUT<int>();
            var id = 5;
            var newCount = 3;

            using (var session = await sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent<CountChangedClassEvent>(x => x.NewCount = newCount);

                await session.SaveChanges();
            }

            //Assert
            sut[id].Should().NotBeNull();
            sut[id].Length.Should().Be(1);
            sut[id][0].Should().BeOfType<CountChangedClassEvent>();
            sut[id][0].As<CountChangedClassEvent>().NewCount.Should().Be(newCount);
        }

        [Fact]
        public async Task SaveEvents_WithOneNewEvent_ShouldPublishEvent()
        {
            //Arrangements
            var config = new ConfigurationStub<TestState>()
                .WithDefaultSetup();
            var sut = config.GetNewSUT<int>();
            var @event = new object();
            var id = 42;

            using (var session = await sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent(@event);
                await session.SaveChanges();
            }

            //Assert
            config.EventsThatHaveBeenPublished.Count.Should().Be(1);
            config.EventsThatHaveBeenPublished[0].Should().Be(@event);
        }

        [Fact]
        public async Task DisposeSession_WithOneEvent_ShouldNotSaveEvents()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();
            var @event = new object();
            var id = 42;

            using (var session = await sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent(@event);
            }

            //Assert
            sut[id].Should().BeEmpty();
        }

        [Fact]
        public async Task DisposeSession_WithOneEvent_ShouldNotPublishEvents()
        {
            //Arrangements
            var config = new ConfigurationStub<TestState>()
                .WithDefaultSetup();
            var sut = config.GetNewSUT<int>();
            var @event = new object();
            var id = 42;

            using (var session = await sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent(@event);
            }

            //Assert
            config.EventsThatHaveBeenPublished.Count.Should().Be(0);
        }

        [Fact]
        public async Task StreamWithOneEvent_Clear_ShouldEmptyStream()
        {
            //Arrange
            int id = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, id);

            //Act
            await sut.Delete(id);

            //Assert
            sut[id].Should().BeEmpty();
        }

        [Fact]
        public async Task StreamWithOneEvent_BeginSessionAndAddOneEvent_ShouldHaveStreamWith2Events()
        {
            //Arrange
            int id = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, id);
            object newEvent = new object();

            //Act
            using (var session = await sut.BeginSessionFor(id))
            {
                session.AddEvent(newEvent);
                await session.SaveChanges();
            }

            //Assert
            sut[id].Length.Should().Be(2);
            sut[id][1].Should().Be(newEvent);
        }

        [Fact]
        public async Task StreamWithOneEvent_ClearThenBeginSessionWiththrowIfNotExist_ShouldThrow()
        {
            //Arrange
            int id = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, id);
            await sut.Delete(id);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.BeginSessionFor(id, true));

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<StreamNotFoundException>();
        }

        [Fact]
        public async Task StreamWithOneEvent_Exists_ShouldReturnTrue()
        {
            //Arrange
            int id = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, id);

            //Act
            var exists = await sut.Contains(id);

            //Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task StreamWithOneEvent_ExistsWithDifferentId_ShouldReturnFalse()
        {
            //Arrange
            int idWithEvent = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, idWithEvent);

            //Act
            var exists = await sut.Contains(idWithEvent+1);

            //Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task StreamWithoutEvents_Exists_ShouldReturnFalse()
        {
            //Arrange
            int id = 42;
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();

            //Act
            var exists = await sut.Contains(id);

            //Assert
            exists.Should().BeFalse();
        }
    }
}
