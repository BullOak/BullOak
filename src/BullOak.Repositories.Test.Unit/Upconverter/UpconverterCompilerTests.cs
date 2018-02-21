namespace BullOak.Repositories.Test.Unit.Upconverter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.Upconverting;
    using FluentAssertions;
    using Xunit;
    using UpconvertFunc = System.Func<BullOak.Repositories.ItemWithType, BullOak.Repositories.Upconverting.UpconvertResult>;

    public class EventA
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public EventA(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }

    public class EventB
    {
        public string MyName { get; set; }
        public int Count { get; set; }

        public EventB(string name, int count)
        {
            MyName = name;
            Count = count;
        }
    }

    public class EventC
    {
        public int Count { get; set; }

        public EventC(int count)
            => Count = count;
    }

    public class EventD
    {
        public int MyCount { get; set; }

        public EventD(int count)
            => MyCount = count;
    }

    public class EventE
    {
        public string Formatted { get; set; }

        public EventE(string formatted)
            => Formatted = formatted;
    }

    public class UpconverterAToB : IUpconvertEvent<EventA, EventB>
    {
        public EventB Upconvert(EventA source)
            => new EventB(source.Name, source.Count);
    }

    public class UpconverterBToE : IUpconvertEvent<EventB, EventE>
    {
        public EventE Upconvert(EventB source)
            => new EventE($"{source.MyName}-{source.Count}");
    }

    public class UpconverterAToBAndC : IUpconvertEvent<EventA>
    {
        public IEnumerable<object> Upconvert(EventA source)
        {
            yield return new EventB(source.Name, source.Count);
            yield return new EventC(source.Count);
        }
    }

    public class UpconverterCToD : IUpconvertEvent<EventC, EventD>
    {
        public EventD Upconvert(EventC source)
            => new EventD(source.Count);
    }

    public class UpconverterWithNonPublicCtor : IUpconvertEvent<EventA>
    {
        private UpconverterWithNonPublicCtor()
        { }

        public IEnumerable<object> Upconvert(EventA source)
            => new List<object>();
    }

    public class UpconverterCompilerTests
    {
        [Fact]
        public void GetFromTypes_WithNoUpconverters_ShouldReturnEmptyDictionary()
        {
            // Arrange
            Dictionary<Type, UpconvertFunc> upconverters = null;

            // Act
            var exception = Record.Exception((Action)(() => upconverters = UpconverterCompiler.GetFrom(new List<Type>())));

            // Assert
            exception.Should().BeNull();
            upconverters.Should().BeEmpty();
        }

        [Fact]
        public void GetFromTypes_WithNullInput_ShouldThrowNullArgumentException()
        {
            // Arrange

            // Act
            var exception = Record.Exception(() => UpconverterCompiler.GetFrom(null));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void GetFromTypes_WithUpconverterWithNoPublicDefaultCtor_ShouldThrowCannotInstantiateUpconverterException()
        {
            // Arrange

            // Act
            var exception = Record.Exception(() => UpconverterCompiler.GetFrom(new [] {typeof(
                UpconverterWithNonPublicCtor) }));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<CannotInstantiateUpconverterException>();
            exception.As<CannotInstantiateUpconverterException>().upconverterType.Should().Be<UpconverterWithNonPublicCtor>();
        }

        [Fact]
        public void GetFromTypes_WithUpconvertersWithSameSourceEvent_ShouldThrowPreflightUpconverterConflictException()
        {
            // Arrange

            // Act
            var exception = Record.Exception(() => UpconverterCompiler.GetFrom(new []
            {
                typeof(UpconverterAToB),
                typeof(UpconverterAToBAndC),
            }));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<AggregateException>();
            exception.As<AggregateException>().Flatten().InnerExceptions.Count.Should().Be(1);
            var innerException = exception.As<AggregateException>().Flatten().InnerExceptions.ToArray()[0];
            innerException.Should().BeOfType<PreflightUpconverterConflictException>();
            innerException.As<PreflightUpconverterConflictException>().SourceEventType.Should().Be<EventA>();
        }

        [Fact]
        public void GetFromTypes_WithTypesButNoUpconverter_ShouldReturnEmptyDictionary()
        {
            // Arrange
            var types = new[] {this.GetType()};
            Dictionary<Type, UpconvertFunc> upconverters = null;

            // Act
            var exception =
                Record.Exception((Action) (() => upconverters = UpconverterCompiler.GetFrom(types)));

            // Assert
            exception.Should().BeNull();
            upconverters.Should().BeEmpty();
        }

        [Fact]
        public void GetFromTypes_WithwithSingleEventUpconverter_ShouldReturnDictionaryWithOneEntry()
        {
            // Arrange
            var types = new[] {typeof(UpconverterAToB)};
            Dictionary<Type, UpconvertFunc> upconverters = null;

            // Act
            upconverters = UpconverterCompiler.GetFrom(types);

            // Assert
            upconverters.Count.Should().Be(1);
            upconverters.FirstOrDefault().Key.Should().Be<EventA>();
        }

        [Fact]
        public void GetFromTypes_WithwithTwoUpconverterInChain_ShouldReturnDictionaryWithTwoEntries()
        {
            // Arrange
            var types = new[] {typeof(UpconverterAToB), typeof(UpconverterBToE)};
            Dictionary<Type, UpconvertFunc> upconverters = null;

            // Act
            upconverters = UpconverterCompiler.GetFrom(types);

            // Assert
            upconverters.Count.Should().Be(2);
        }

        [Fact]
        public void
            GetFromTypes_WithwithOneUpconverterInChainFromEventAToEventB_ShouldReturnDictionaryWithOneUpconvertFromEventAToEventB()
        {
            // Arrange
            var types = new[] {typeof(UpconverterAToB)};
            UpconvertFunc upconverter = null;
            var originalEvent = new EventA("Mr. Silly Name",5);
            var expectedEvent = new EventB(originalEvent.Name, originalEvent.Count);

            // Act
            upconverter = UpconverterCompiler.GetFrom(types)[typeof(EventA)];

            // Assert
            upconverter.Should().NotBeNull();
            upconverter(new ItemWithType(originalEvent)).isSingleItem.Should().BeTrue();
            upconverter(new ItemWithType(originalEvent)).single.type.Should().Be<EventB>();
            upconverter(new ItemWithType(originalEvent)).single.instance.As<EventB>().MyName.Should().Be(expectedEvent.MyName);
            upconverter(new ItemWithType(originalEvent)).single.instance.As<EventB>().Count.Should().Be(expectedEvent.Count);
        }

        [Fact]
        public void
            GetFromTypes_WithwithTwoUpconverterInChainFromEventAToEventBToEventE_ShouldReturnDictionaryWithUpconvertFromEventAToEventB()
        {
            // Arrange
            var types = new[] {typeof(UpconverterAToB), typeof(UpconverterBToE)};
            UpconvertFunc upconverter = null;
            var originalEvent = new EventA("Mr. Silly Name", 5);
            var wrappedOriginal = new ItemWithType(originalEvent);
            var expectedEvent = new EventB(originalEvent.Name, originalEvent.Count);

            // Act
            upconverter = UpconverterCompiler.GetFrom(types)[typeof(EventA)];

            // Assert
            upconverter.Should().NotBeNull();
            upconverter(wrappedOriginal).isSingleItem.Should().BeTrue();
            upconverter(wrappedOriginal).single.type.Should().Be<EventB>();
            upconverter(wrappedOriginal).single.instance.As<EventB>().MyName.Should().Be(expectedEvent.MyName);
            upconverter(wrappedOriginal).single.instance.As<EventB>().Count.Should().Be(expectedEvent.Count);
        }

        [Fact]
        public void GetFromTypes_WithwithTwoUpconverterInChainFromEventAToEventBToEventE_ShouldReturnDictionaryWithUpconvertFromEventBToEventE()
        {
            // Arrange
            var types = new[] {typeof(UpconverterAToB), typeof(UpconverterBToE)};
            UpconvertFunc upconverter = null;
            var originalEvent = new EventB("Mr. Silly Name", 5);
            var wrappedOriginal = new ItemWithType(originalEvent);
            var expectedEvent = new EventE("Mr. Silly Name-5");

            // Act
            upconverter = UpconverterCompiler.GetFrom(types)[typeof(EventB)];

            // Assert
            upconverter.Should().NotBeNull();
            upconverter(wrappedOriginal).isSingleItem.Should().BeTrue();
            upconverter(wrappedOriginal).single.type.Should().Be<EventE>();
            upconverter(wrappedOriginal).single.instance.As<EventE>().Formatted.Should().Be(expectedEvent.Formatted);
        }

        [Fact]
        public void GetFromTypes_WithwithOneMultiEventUpconverter_ShouldReturnDictionaryWithOneUpconvert()
        {
            var types = new[] {typeof(UpconverterAToBAndC)};
            Dictionary<Type, UpconvertFunc> upconverters = null;

            // Act
            upconverters = UpconverterCompiler.GetFrom(types);

            // Assert
            upconverters.Count.Should().Be(1);
            upconverters.Single().Key.Should().Be<EventA>();
        }

        [Fact]
        public void GetFromTypes_WithwithOneMultiEventUpconverterFromAToBAndC_ShouldReturnDictionaryWithUpconvertFromAToEnumerableWithBAndC()
        {
            var types = new[] {typeof(UpconverterAToBAndC)};
            UpconvertFunc upconverter = null;
            var originalEvent = new EventA("Mr. Silly Name", 5);
            var expectedB = new EventB(originalEvent.Name, originalEvent.Count);
            var expectedC = new EventC(originalEvent.Count);

            // Act
            upconverter = UpconverterCompiler.GetFrom(types)[typeof(EventA)];
            var result = upconverter(new ItemWithType(originalEvent));

            // Assert
            upconverter.Should().NotBeNull();
            result.isSingleItem.Should().BeFalse();
            result.multiple.ToArray()[0].type.Should().Be<EventB>();
            result.multiple.ToArray()[0].instance.As<EventB>().Count.Should().Be(expectedB.Count);
            result.multiple.ToArray()[0].instance.As<EventB>().MyName.Should().Be(expectedB.MyName);
            result.multiple.ToArray()[1].type.Should().Be<EventC>();
            result.multiple.ToArray()[1].instance.As<EventC>().Count.Should().Be(expectedC.Count);
        }
    }
}
