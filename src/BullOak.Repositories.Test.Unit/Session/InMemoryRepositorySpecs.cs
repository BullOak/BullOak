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
    using BullOak.Repositories.StateEmit;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    internal class TestState
    {
        public int Count { get; set; }
        public string Name { get; set; }
    }

    public static class InMemoryEventSourcedRepository
    {
        public static InMemoryEventSourcedRepository<TState,TId> WithEventInStream<TState, TId>(
            this InMemoryEventSourcedRepository<TState, TId> sut, object @event, TId streamId)
        {
            var currentEvents = sut[streamId]?.ToList() ?? new List<object>();
            currentEvents.Add(@event);
            sut[streamId] = currentEvents.ToArray();
            return sut;
        }
    }
    public class ConfigurationStub : IHoldAllConfiguration
    {
        private static readonly Type typeOfGenericList = typeof(List<>);

        private readonly List<Type> typesForWhichEventCollectionHasBeenAskedFor;
        private readonly List<object> eventsThatHaveBeenPublished;
        private readonly List<Type> typesThatHaveAskedForSafetyType;
        private readonly Fake<IApplyEventsToStates> mockEventApplier;
        private readonly Fake<ICreateStateInstances> mockStateFactory;

        public List<Type> TypesForWhichEventCollectionHasBeenAskedFor => typesForWhichEventCollectionHasBeenAskedFor;
        public List<object> EventsThatHaveBeenPublished => eventsThatHaveBeenPublished;
        public List<Type> TypesThatHaveAskedForSafetyType => typesThatHaveAskedForSafetyType;
        public Fake<IApplyEventsToStates> MockEventApplier => mockEventApplier;
        public Fake<ICreateStateInstances> MockStateFactory => mockStateFactory;

        public Func<Type, Func<ICollection<object>>> CollectionTypeSelector { get; private set; }
        public Func<object, Task> EventPublisher { get; private set; }
        public IApplyEventsToStates EventApplier => MockEventApplier.FakedObject;
        public Func<Type, bool> ThreadSafetySelector { get; private set; }
        public ICreateStateInstances StateFactory => MockStateFactory.FakedObject;

        public ConfigurationStub()
        {
            mockStateFactory = new Fake<ICreateStateInstances>();
            mockEventApplier = new Fake<IApplyEventsToStates>();

            typesForWhichEventCollectionHasBeenAskedFor = new List<Type>();
            eventsThatHaveBeenPublished = new List<object>();
            typesThatHaveAskedForSafetyType = new List<Type>();
        }

        public ConfigurationStub WithDefaultSetup()
        {
            EventPublisher = e =>
            {
                eventsThatHaveBeenPublished.Add(e);
                return Task.CompletedTask;
            };
            WithCollectionType<List<object>>();
            WithThreadSafety(false);
            WithStateFactory(Activator.CreateInstance);

            return this;
        }

        public ConfigurationStub WithCollectionType<TCollection>()
        {
            CollectionTypeSelector = t =>
            {
                typesForWhichEventCollectionHasBeenAskedFor.Add(t);
                return () => new List<object>();
            };
            return this;
        }

        public ConfigurationStub WithThreadSafety(bool threadSafety)
        {
            ThreadSafetySelector = t =>
            {
                TypesThatHaveAskedForSafetyType.Add(t);
                return threadSafety;
            };
            return this;
        }

        public ConfigurationStub WithStateFactory(Func<Type, object> factory)
        {
            MockStateFactory.CallsTo(i => i.GetState(null))
                .WithAnyArguments()
                .ReturnsLazily(t => factory((Type)t.Arguments[0]));

            return this;
        }

        public InMemoryEventSourcedRepository<TState, TStateId> GetNewSUT<TState, TStateId>()
            => new InMemoryEventSourcedRepository<TState, TStateId>(this);
    }


    public class InMemoryRepositorySpecs
    {
        [Fact]
        public async Task BeginSession_WithNewIdAndThrowIfNotExist_ShouldThrowException()
        {
            //Arrangements
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>();

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
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>();

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
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>();
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
        public async Task SaveEvents_WithOneNewEvent_ShouldPublishEvent()
        {
            //Arrangements
            var config = new ConfigurationStub()
                .WithDefaultSetup();
            var sut = config.GetNewSUT<TestState, int>();
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
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>();
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
            var config = new ConfigurationStub()
                .WithDefaultSetup();
            var sut = config.GetNewSUT<TestState, int>();
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
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>()
                .WithEventInStream(@event, id);

            //Act
            sut.Clear(id);

            //Assert
            sut[id].Should().BeEmpty();
        }

        [Fact]
        public async Task StreamWithOneEvent_BeginSessionAndAddOneEvent_ShouldHaveStreamWith2Events()
        {
            //Arrange
            int id = 42;
            object @event = new object();
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>()
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
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>()
                .WithEventInStream(@event, id);
            await sut.Clear(id);

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
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>()
                .WithEventInStream(@event, id);

            //Act
            var exists = await sut.Exists(id);

            //Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task StreamWithOneEvent_ExistsWithDifferentId_ShouldReturnFalse()
        {
            //Arrange
            int idWithEvent = 42;
            object @event = new object();
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>()
                .WithEventInStream(@event, idWithEvent);

            //Act
            var exists = await sut.Exists(idWithEvent+1);

            //Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task StreamWithoutEvents_Exists_ShouldReturnFalse()
        {
            //Arrange
            int id = 42;
            var sut = new ConfigurationStub()
                .WithDefaultSetup()
                .GetNewSUT<TestState, int>();

            //Act
            var exists = await sut.Exists(id);

            //Assert
            exists.Should().BeFalse();
        }

    }
}
