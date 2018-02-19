namespace BullOak.Repositories.Test.Unit.Upconverter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using BullOak.Repositories.Config;
    using BullOak.Repositories.Middleware;
    using BullOak.Repositories.Upconverting;
    using FluentAssertions;
    using Xunit;

    public class UpconverterConfigTests
    {
        private class ConfigurationStub : IConfigureUpconverter, IBuildConfiguration
        {
            private IUpconvertStoredItems upconverter;

            public KeyValuePair<Type, Func<ItemWithType, UpconvertResult>>[] GetUpconvertFunctions() 
                => GetUpconvertersFrom(upconverter);

            public IHoldAllConfiguration Build()
                => null;

            public IBuildConfiguration WithUpconverter(IUpconvertStoredItems upconverter)
            {
                this.upconverter = upconverter;
                return this;
            }

            private KeyValuePair<Type, Func<ItemWithType, UpconvertResult>>[] GetUpconvertersFrom(
                IUpconvertStoredItems upconvertEngine)
            {
                var field = typeof(EventUpconverter)
                    .GetField("upconverters", BindingFlags.Instance | BindingFlags.NonPublic);

                var upconverters =
                    (IReadOnlyDictionary<Type, Func<ItemWithType, UpconvertResult>>) field.GetValue(upconvertEngine);

                return upconverters.ToArray();
            }

            public void AddInterceptor(IInterceptEvents interceptor)
            { }
        }

        [Fact]
        public void WithUpconvertersFrom_NullTypeList_ShouldThrowArgumentNullException()
        {
            // Arrange
            var sut = new ConfigurationStub();

            // Act
            var exception = Record.Exception(() => sut.WithUpconvertersFrom((IEnumerable<Type>) null));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void WithUpconvertersFrom_NullAssembly_ShouldThrowArgumentNullException()
        {
            // Arrange
            var sut = new ConfigurationStub();

            // Act
            var exception = Record.Exception(() => sut.WithUpconvertersFrom((Assembly) null));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void WithUpconvertersFrom_OneUpconverterTypeAndOneOtherType_ShouldNotThrowException()
        {
            // Arrange
            var sut = new ConfigurationStub();
            var data = new Type[]
            {
                GetType(),
                typeof(UpconverterAToB)
            };

            // Act
            var build = sut.WithUpconvertersFrom(data)
                .AndNoMoreUpconverters();
            var exception = Record.Exception(() => build.Build());

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public void WithUpconvertersFrom_OneUpconverterTypeAndOneOtherType_ShouldAddOneUpconverterInList()
        {
            // Arrange
            var sut = new ConfigurationStub();
            var data = new Type[]
            {
                GetType(),
                typeof(UpconverterAToB)
            };

            // Act
            sut.WithUpconvertersFrom(data)
                .AndNoMoreUpconverters()
                .Build();

            // Assert
            var upconvertFuncs = sut.GetUpconvertFunctions();
            upconvertFuncs.Should().NotBeNull();
            upconvertFuncs.Length.Should().Be(1);
            upconvertFuncs[0].Key.Should().Be<EventA>();
        }

        [Fact]
        public void WithUpconvertersFrom_UpconverterContainerAssembly_ShouldNotThrowException()
        {
            // Arrange
            var sut = new ConfigurationStub();

            // Act
            var exception = Record.Exception(() => sut.WithUpconvertersFrom(typeof(BullOak.Repositories.Test.Unit.UpconverterContainer.EventA).Assembly)
                .AndNoMoreUpconverters()
                .Build());

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public void WithUpconvertersFrom_UpconverterContainerAssembly_ShouldLoadPublicUpconvertersOfPublicEvents()
        {
            // Arrange
            var sut = new ConfigurationStub();

            // Act
            sut.WithUpconvertersFrom(typeof(BullOak.Repositories.Test.Unit.UpconverterContainer.EventA).Assembly)
                .AndNoMoreUpconverters()
                .Build();

            // Assert
            sut.GetUpconvertFunctions()
                .Any(x => x.Key == typeof(UpconverterContainer.EventA))
                .Should().BeTrue();
            sut.GetUpconvertFunctions()
                .Any(x => x.Key == typeof(UpconverterContainer.EventB))
                .Should().BeTrue();
        }

        [Fact]
        public void WithUpconvertersFrom_UpconverterContainerAssembly_ShouldLoadInternalUpconvertersOfPublicEvents()
        {
            // Arrange
            var sut = new ConfigurationStub();

            // Act
            sut.WithUpconvertersFrom(typeof(BullOak.Repositories.Test.Unit.UpconverterContainer.EventA).Assembly)
                .AndNoMoreUpconverters()
                .Build();

            // Assert
            sut.GetUpconvertFunctions()
                .Any(x => x.Key == typeof(UpconverterContainer.EventC))
                .Should().BeTrue();
        }

        [Fact]
        public void WithUpconvertersFrom_UpconverterContainerAssembly_ShouldLoadInternalUpconvertersOfInternalEvents()
        {
            // Arrange
            var sut = new ConfigurationStub();

            // Act
            sut.WithUpconvertersFrom(typeof(BullOak.Repositories.Test.Unit.UpconverterContainer.EventA).Assembly)
                .AndNoMoreUpconverters()
                .Build();

            // Assert
            sut.GetUpconvertFunctions()
                .Any(x => x.Key.Name.Equals("InternalEvent"))
                .Should().BeTrue();
        }
    }
}
