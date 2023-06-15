namespace BullOak.Repositories.Test.Unit.Session
{
    using Appliers;
    using BullOak.Repositories.Session;
    using Exceptions;
    using FluentAssertions;
    using InMemory;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Xunit;

    public static class InMemoryEventSourcedRepositoryTestExtensions
    {
        public static InMemoryEventSourcedRepository<TId, TState> WithEventInStream<TState, TId>(
            this InMemoryEventSourcedRepository<TId, TState> sut,
            object @event,
            TId streamId)
        {
            var currentEvents = sut[streamId]?.ToList() ?? new List<(StoredEvent, DateTime)>();
            currentEvents.Add((new StoredEvent(@event.GetType(), @event, currentEvents.Count), DateTime.UtcNow));
            sut[streamId] = currentEvents.ToArray();
            return sut;
        }
    }

    public class InMemoryRepositorySpecs
    {
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
        public async Task BeginSession_WithNewIdAndNotThrow_ShouldNotThrow()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();

            //Act
            var exception = await Record.ExceptionAsync(() => sut.BeginSessionFor(123, throwIfNotExists: false));

            //Assert
            exception.Should().BeNull();
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
            sut[id][0].Item1.Event.Should().Be(@event);
        }

        [Fact]
        public async Task SaveEvents_WithOneNewInitializedInterfaceEvent_ShouldAddEventInStore()
        {
            //Arrangements
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .WithDefaultStateFactory()
                .WithEventApplier(new CountChangedApplier())
                .GetNewSUT<int>();
            var id = 5;
            var newCount = 3;

            using (var session = await sut.BeginSessionFor(id))
            {
                //Act
                session.AddEvent<ICountChangedInterfaceEvent>(x => x.NewCount = newCount);

                await session.SaveChanges();
            }

            //Assert
            sut[id].Should().NotBeNull();
            sut[id].Length.Should().Be(1);
            sut[id][0].Item1.Event.Should().BeAssignableTo<ICountChangedInterfaceEvent>();
            sut[id][0].Item1.Event.As<ICountChangedInterfaceEvent>().NewCount.Should().Be(newCount);
        }

        [Fact]
        public async Task SaveEvents_WithOneNewInitializedClassEvent_ShouldAddEventInStore()
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
            sut[id][0].Item1.Event.Should().BeOfType<CountChangedClassEvent>();
            sut[id][0].Item1.Event.As<CountChangedClassEvent>().NewCount.Should().Be(newCount);
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
        public async Task StreamWithOneEvent_BeginSessionWithThrowIfNotExists_ShouldNotThrow()
        {
            //Arrange
            int id = 42;
            object @event = new object();
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>()
                .WithEventInStream(@event, id);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.BeginSessionFor(id, true));

            //Assert
            exception.Should().BeNull();
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
            sut[id][1].Item1.Event.Should().Be(newEvent);
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

        [Fact]
        public async Task WhenAValidatorIsProvided_StartSession_ShouldProvideSessionWithValidator()
        {
            //Arrange
            int id = 42;
            var repo = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>(new AlwaysFailValidator<TestState>());
            var sut = await repo.BeginSessionFor(id);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.SaveChanges());

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<AggregateException>();
            (exception as AggregateException).InnerExceptions[0].Should().BeOfType<BusinessException>();
        }

        [Fact]
        public void WhenAValidatorIsNotProvided_RepositoryShouldHaveAlwaysPassValidator()
        {
            //Arrange
            var sut = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();

            //Act
            var defaultValidator = sut.GetType()
                .GetField("stateValidator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sut);

            //Assert
            defaultValidator.Should().NotBeNull();
            defaultValidator.GetType().Should().Be<AlwaysPassValidator<TestState>>();
        }

        [Fact]
        public async Task WhenAValidatorIsNotProvided_StartSession_ShouldProvideSessionWithAlwaysPassValidator()
        {
            //Arrange
            var repo = new ConfigurationStub<TestState>()
                .WithDefaultSetup()
                .GetNewSUT<int>();
            var sut = await repo.BeginSessionFor(42);

            //Act
            var defaultValidator = (sut as InMemoryEventStoreSession<TestState, int>).StateValidator;

            //Assert
            defaultValidator.Should().NotBeNull();
            defaultValidator.GetType().Should().Be<AlwaysPassValidator<TestState>>();
        }

        public class AlwaysFailValidator<TState> : IValidateState<TState>
        {
            /// <inheritdoc />
            public ValidationResults Validate(TState state)
                => ValidationResults.Errors(new BasicValidationError[] {"BasicErrorInYourValidation."});
        }

    }
}
