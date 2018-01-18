namespace BullOak.Repositories.Test.Unit.Upconverter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.Upconverting;
    using FluentAssertions;
    using Xunit;
    using UpconvertFunc = System.Func<Upconverting.ItemWithType, Upconverting.UpconvertResult>;

    public class EventUpconverterTests
    {
        internal class Arrangements
        {
            private List<KeyValuePair<Type, UpconvertFunc>> upconverters = new List<KeyValuePair<Type, UpconvertFunc>>();

            public Arrangements AddUpconverter<TSource, TDestination>(Func<TSource, TDestination> upconverter)
            {
                upconverters.Add(new KeyValuePair<Type, UpconvertFunc>(typeof(TSource),
                    i => new UpconvertResult(new ItemWithType(upconverter((TSource) i.instance)))));
                return this;
            }

            public Arrangements AddUpconverter<TSource>(Func<TSource, IEnumerable<object>> upconverter)
            {
                upconverters.Add(new KeyValuePair<Type, UpconvertFunc>(typeof(TSource),
                    i => new UpconvertResult(upconverter((TSource) i.instance).Select(x=> new ItemWithType(x)))));
                return this;
            }

            public EventUpconverter BuildAndGetSUT()
                => (EventUpconverter) upconverters.ToDictionary(x => x.Key, x => x.Value);
        }

        [Fact]
        public void Upconvert_WithNull_ShouldThrowException()
        {
            // Arrange
            var sut = new Arrangements().BuildAndGetSUT();

            // Act
            var exception = Record.Exception(() => sut.Upconvert(null));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void Upconvert_WithEmptyInput_ShouldReturnEmptyCollectionAndNotThrow()
        {
            // Arrange
            var sut = new Arrangements().BuildAndGetSUT();
            IEnumerable<ItemWithType> upconverted = null;

            // Act
            var exception = Record.Exception((Action)(() => upconverted = sut.Upconvert(new ItemWithType[0])));

            // Assert
            exception.Should().BeNull();
            upconverted.Should().NotBeNull();
            upconverted.Count().Should().Be(0);
        }


        [Fact]
        public void UpconvertGivenUpconverterAToB_WithEventA_ShouldReturnEventB()
        {
            // Arrange
            var sut = new Arrangements()
                .AddUpconverter<EventA, EventB>(a => new EventB(a.Name, a.Count))
                .BuildAndGetSUT();
            var source = new EventA("Mr. Silly Name", 5);
            var expected = new EventB(source.Name, source.Count);

            // Act
            var upconverted = sut.Upconvert(new ItemWithType[] {new ItemWithType(source)});

            // Assert
            upconverted.Should().NotBeNullOrEmpty();
            upconverted.Count().Should().Be(1);
            upconverted.First().type.Should().Be<EventB>();
            upconverted.First().instance.As<EventB>().MyName.Should().Be(expected.MyName);
            upconverted.First().instance.As<EventB>().Count.Should().Be(expected.Count);
        }

        [Fact]
        public void UpconvertGivenUpconverterAToB_WithEventB_ShouldReturnEventB()
        {
            // Arrange
            var sut = new Arrangements()
                .AddUpconverter<EventA, EventB>(a => new EventB(a.Name, a.Count))
                .BuildAndGetSUT();
            var source = new EventB("Mr. Silly Name", 5);

            // Act
            var upconverted = sut.Upconvert(new ItemWithType[] {new ItemWithType(source)});

            // Assert
            upconverted.Should().NotBeNullOrEmpty();
            upconverted.Count().Should().Be(1);
            upconverted.First().type.Should().Be<EventB>();
            ReferenceEquals(upconverted.First().instance, source).Should().BeTrue();
        }

        [Fact]
        public void UpconvertGivenUpconverterAToBAndC_WithEventA_ShouldReturnEventBAndEventC()
        {
            // Arrange
            var sut = new Arrangements()
                .AddUpconverter<EventA>(UpconvertMethod)
                .BuildAndGetSUT();
            var source = new EventA("Mr. Silly Name", 4);
            var expectedB = new EventB(source.Name, source.Count);
            var expectedC = new EventC(source.Count);

            // Act
            var upconverted = sut.Upconvert(new ItemWithType[] {new ItemWithType(source)});

            // Assert
            upconverted.Should().NotBeNullOrEmpty();
            upconverted.Count().Should().Be(2);
            upconverted.ToArray()[0].type.Should().Be<EventB>();
            upconverted.ToArray()[0].instance.As<EventB>().MyName.Should().Be(expectedB.MyName);
            upconverted.ToArray()[0].instance.As<EventB>().Count.Should().Be(expectedB.Count);
            upconverted.ToArray()[1].type.Should().Be<EventC>();
            upconverted.ToArray()[1].instance.As<EventC>().Count.Should().Be(expectedC.Count);

            IEnumerable<object> UpconvertMethod(EventA a)
            {
                yield return new EventB(a.Name, a.Count);
                yield return new EventC(a.Count);
            }
        }

        [Fact]
        public void UpconvertGivenUpconverterAToBAndUpconvertBToC_WithEventA_ShouldReturnEventC()
        {
            // Arrange
            var sut = new Arrangements()
                .AddUpconverter<EventA, EventB>(x => new EventB(x.Name, x.Count))
                .AddUpconverter<EventB, EventC>(x => new EventC(x.Count))
                .BuildAndGetSUT();
            var source = new EventA("Mr. Silly Name", 4);
            var expected = new EventC(source.Count);

            // Act
            var upconverted = sut.Upconvert(new ItemWithType[] {new ItemWithType(source)});

            // Assert
            upconverted.Should().NotBeNullOrEmpty();
            upconverted.Count().Should().Be(1);
            upconverted.ToArray()[0].type.Should().Be<EventC>();
            upconverted.ToArray()[0].instance.As<EventC>().Count.Should().Be(expected.Count);
        }

        [Fact]
        public void UpconvertGivenUpconverterAToBAndUpconvertBToC_WithEventB_ShouldReturnEventC()
        {
            // Arrange
            var sut = new Arrangements()
                .AddUpconverter<EventA, EventB>(x => new EventB(x.Name, x.Count))
                .AddUpconverter<EventB, EventC>(x => new EventC(x.Count))
                .BuildAndGetSUT();
            var source = new EventB("Mr. Silly Name", 4);
            var expected = new EventC(source.Count);

            // Act
            var upconverted = sut.Upconvert(new ItemWithType[] {new ItemWithType(source)});

            // Assert
            upconverted.Should().NotBeNullOrEmpty();
            upconverted.Count().Should().Be(1);
            upconverted.ToArray()[0].type.Should().Be<EventC>();
            upconverted.ToArray()[0].instance.As<EventC>().Count.Should().Be(expected.Count);
        }

        [Fact]
        public void UpconvertGivenUpconverterAToBAndCAndUpconvertCToD_WithEventA_ShouldReturnEventBAndEventD()
        {
            // Arrange
            var sut = new Arrangements()
                .AddUpconverter<EventA>(UpconvertMethod)
                .AddUpconverter<EventC, EventD>(x => new EventD(x.Count))
                .BuildAndGetSUT();
            var source = new EventA("Mr. Silly Name", 4);
            var expectedB = new EventB(source.Name, source.Count);
            var expectedD = new EventD(source.Count);

            // Act
            var upconverted = sut.Upconvert(new [] {new ItemWithType(source)});

            // Assert
            upconverted.Should().NotBeNullOrEmpty();
            upconverted.Count().Should().Be(2);
            upconverted.ToArray()[0].type.Should().Be<EventB>();
            upconverted.ToArray()[0].instance.As<EventB>().MyName.Should().Be(expectedB.MyName);
            upconverted.ToArray()[0].instance.As<EventB>().Count.Should().Be(expectedB.Count);
            upconverted.ToArray()[1].type.Should().Be<EventD>();
            upconverted.ToArray()[1].instance.As<EventD>().MyCount.Should().Be(expectedD.MyCount);

            IEnumerable<object> UpconvertMethod(EventA a)
            {
                yield return new EventB(a.Name, a.Count);
                yield return new EventC(a.Count);
            }
        }
    }
}
