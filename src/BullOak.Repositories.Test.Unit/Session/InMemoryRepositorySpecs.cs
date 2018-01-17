namespace BullOak.Repositories.Test.Unit.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventPublisher;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.InMemory;
    using BullOak.Repositories.Session;
    using BullOak.Repositories.StateEmit;
    using BullOak.Repositories.Upconverting;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    internal class TestState
    {
        public int Count { get; set; }
        public string Name { get; set; }
    }

    public static class InMemoryEventSourcedRepositoryTestExtensions
    {
        public static InMemoryEventSourcedRepository<TId, TState> WithEventInStream<TState, TId>(
            this InMemoryEventSourcedRepository<TId, TState> sut, object @event, TId streamId)
        {
            var currentEvents = sut[streamId]?.ToList() ?? new List<object>();
            currentEvents.Add(@event);
            sut[streamId] = currentEvents.ToArray();
            return sut;
        }
    }
    public class ConfigurationStub<TState> : IHoldAllConfiguration
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
        public IPublishEvents EventPublisher { get; private set; }
        public IApplyEventsToStates EventApplier => MockEventApplier.FakedObject;
        public Func<Type, bool> ThreadSafetySelector { get; private set; }
        public ICreateStateInstances StateFactory => MockStateFactory.FakedObject;

        public IUpconvertStoredItems EventUpconverter { get; private set; }

        public ConfigurationStub()
        {
            mockStateFactory = new Fake<ICreateStateInstances>();
            mockEventApplier = new Fake<IApplyEventsToStates>();
            EventUpconverter = new NullUpconverter();

            typesForWhichEventCollectionHasBeenAskedFor = new List<Type>();
            eventsThatHaveBeenPublished = new List<object>();
            typesThatHaveAskedForSafetyType = new List<Type>();
        }

        public ConfigurationStub<TState> WithDefaultSetup()
        {
            EventPublisher = new MySyncEventPublisher(e =>
            {
                eventsThatHaveBeenPublished.Add(e);
            });
            WithCollectionType<List<object>>();
            WithThreadSafety(false);
            WithStateFactory(Activator.CreateInstance);
            WithJustReturnEventApplier();

            return this;
        }

        private void WithJustReturnEventApplier()
        {
            mockEventApplier.CallsTo(a => a.ApplyEvent(typeof(object), new object(), new object()))
                .WithAnyArguments()
                .ReturnsLazily(c => c.Arguments.ToArray()[1]);
            mockEventApplier.CallsTo(a => a.Apply(typeof(object), new object(), new object[0]))
                .WithAnyArguments()
                .ReturnsLazily(c => c.Arguments.ToArray()[1]);
            mockEventApplier.CallsTo(a => a.Apply(typeof(object), new object(), new List<object>()))
                .WithAnyArguments()
                .ReturnsLazily(c => c.Arguments.ToArray()[1]);
        }

        public ConfigurationStub<TState> WithCollectionType<TCollection>()
        {
            CollectionTypeSelector = t =>
            {
                typesForWhichEventCollectionHasBeenAskedFor.Add(t);
                return () => new List<object>();
            };
            return this;
        }

        public ConfigurationStub<TState> WithThreadSafety(bool threadSafety)
        {
            ThreadSafetySelector = t =>
            {
                TypesThatHaveAskedForSafetyType.Add(t);
                return threadSafety;
            };
            return this;
        }

        public ConfigurationStub<TState> WithStateFactory(Func<Type, object> factory)
        {
            MockStateFactory.CallsTo(i => i.GetState(null))
                .WithAnyArguments()
                .ReturnsLazily(t => factory((Type)t.Arguments[0]));

            return this;
        }

        internal InMemoryEventSourcedRepository<TStateId, TState> GetNewSUT<TStateId>()
            => new InMemoryEventSourcedRepository<TStateId, TState>(this);
    }


    public class InMemoryRepositorySpecs
    {
        [Fact]
        public void BeginSession_WithNewIdAndThrowIfNotExist_ShouldThrowException()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();

            //Act
            var exception = Record.Exception(() => sut.BeginSessionFor(123, throwIfNotExists: true));

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<StreamNotFoundException>();
        }

        [Fact]
        public void BeginSession_WithNewId_ShouldReturnSession()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();

            //Act
            var session = sut.BeginSessionFor(123);

            //Assert
            session.Should().NotBeNull();
            session.Should().BeAssignableTo<IManageSessionOf<TestState>>();
        }

        [Fact]
        public void SaveEvents_WithOneNewEvent_ShouldAddEventInStore()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();
            var @event = new object();
            var id = 42;

            using (var session = sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent(@event);
                session.SaveChanges();
            }

            //Assert
            sut[id].Should().NotBeNull();
            sut[id].Length.Should().Be(1);
            sut[id][0].Should().Be(@event);
        }

        [Fact]
        public void SaveEvents_WithOneNewEvent_ShouldPublishEvent()
        {
            //Arrangements
            var config = new ConfigurationStub<TestState>()
                .WithDefaultSetup();
            var sut = config.GetNewSUT<int>();
            var @event = new object();
            var id = 42;

            using (var session = sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent(@event);
                session.SaveChanges();
            }

            //Assert
            config.EventsThatHaveBeenPublished.Count.Should().Be(1);
            config.EventsThatHaveBeenPublished[0].Should().Be(@event);
        }

        [Fact]
        public void DisposeSession_WithOneEvent_ShouldNotSaveEvents()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();
            var @event = new object();
            var id = 42;

            using (var session = sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent(@event);
            }

            //Assert
            sut[id].Should().BeEmpty();
        }

        [Fact]
        public void DisposeSession_WithOneEvent_ShouldNotPublishEvents()
        {
            //Arrangements
            var config = new ConfigurationStub<TestState>()
                .WithDefaultSetup();
            var sut = config.GetNewSUT<int>();
            var @event = new object();
            var id = 42;

            using (var session = sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent(@event);
            }

            //Assert
            config.EventsThatHaveBeenPublished.Count.Should().Be(0);
        }

        [Fact]
        public void StreamWithOneEvent_Clear_ShouldEmptyStream()
        {
            //Arrange
            int id = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, id);

            //Act
            sut.Clear(id);

            //Assert
            sut[id].Should().BeEmpty();
        }

        [Fact]
        public void StreamWithOneEvent_BeginSessionAndAddOneEvent_ShouldHaveStreamWith2Events()
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
            using (var session = sut.BeginSessionFor(id))
            {
                session.AddEvent(newEvent);
                session.SaveChanges();
            }

            //Assert
            sut[id].Length.Should().Be(2);
            sut[id][1].Should().Be(newEvent);
        }

        [Fact]
        public void StreamWithOneEvent_ClearThenBeginSessionWiththrowIfNotExist_ShouldThrow()
        {
            //Arrange
            int id = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, id);
            sut.Clear(id);

            //Act
            var exception = Record.Exception(() => sut.BeginSessionFor(id, true));

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<StreamNotFoundException>();
        }

        [Fact]
        public void StreamWithOneEvent_Exists_ShouldReturnTrue()
        {
            //Arrange
            int id = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, id);

            //Act
            var exists = sut.Exists(id);

            //Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public void StreamWithOneEvent_ExistsWithDifferentId_ShouldReturnFalse()
        {
            //Arrange
            int idWithEvent = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, idWithEvent);

            //Act
            var exists = sut.Exists(idWithEvent+1);

            //Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public void StreamWithoutEvents_Exists_ShouldReturnFalse()
        {
            //Arrange
            int id = 42;
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();

            //Act
            var exists = sut.Exists(id);

            //Assert
            exists.Should().BeFalse();
        }
    }
}
