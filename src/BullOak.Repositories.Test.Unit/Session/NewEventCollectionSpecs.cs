namespace BullOak.Repositories.Test.Unit.Session
{
    using System;
    using System.Linq;
    using BullOak.Repositories.Session;
    using BullOak.Repositories.Session.CustomLinkedList;
    using FluentAssertions;
    using Xunit;

    public class NewEventCollectionSpecs
    {
        [Fact]
        public void AddEvent_WithNullEvent_ThrowsNullReferenceException()
        {
            //Arrange
            var sut = new LinkedList<object>();

            //Act
            var exception = Record.Exception(() => sut.Add(null));

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void AddEvent_WithEmptyCollection_ShouldNotThrow()
        {
            //Arrange
            var sut = new LinkedList<object>();

            //Act
            var exception = Record.Exception(() => sut.Add(new object()));

            //Assert
            exception.Should().BeNull();
        }

        [Fact]
        public void AddEvent_WithExistingEvents_ShouldNotThrow()
        {
            //Arrange
            var sut = new LinkedList<object>();
            sut.Add(new object());
            sut.Add(new object());

            //Act
            var exception = Record.Exception(() => sut.Add(new object()));

            //Assert
            exception.Should().BeNull();
        }

        [Fact]
        public void AddEvent_WithEmptyCollection_AddsEventToBuffer()
        {
            //Arrange
            var sut = new LinkedList<object>();
            var @event = new object();

            //Act
            sut.Add(@event);

            //Assert
            // yes this calls GetBuffer, but there is no way to reliably test enquing otherwise
            sut.GetBuffer().Length.Should().Be(1);
            sut.GetBuffer()[0].Should().Be(@event);
        }

        [Fact]
        public void GetEventBuffer_WithEmptyCollection_ShouldReturnEmptyArray()
        {
            //Arrange
            var sut = new LinkedList<object>();

            //Act
            var buffer = sut.GetBuffer();

            //Assert
            buffer.Should().NotBeNull();
            buffer.Length.Should().Be(0);
        }

        [Fact]
        public void GetEventBuffer_WithEmptyCollection_ShouldNotThrow()
        {
            //Arrange
            var sut = new LinkedList<object>();

            //Act
            var exception = Record.Exception(() => sut.GetBuffer());

            //Assert
            exception.Should().BeNull();
        }

        [Fact]
        public void GetEventBuffer_With3Events_ShouldReturnBufferWith3EventsInOrder()
        {
            //Arrange
            var sut = new LinkedList<object>();
            object e1 = new object(), e2 = new object(), e3 = new object();
            sut.Add(e1);
            sut.Add(e2);
            sut.Add(e3);

            //Act
            var buffer = sut.GetBuffer();

            //Assert
            buffer.Length.Should().Be(3);
            buffer[0].Should().Be(e1);
            buffer[1].Should().Be(e2);
            buffer[2].Should().Be(e3);
        }

        [Fact]
        public void GetEnumerator_With3Events_ShouldReturnFunctioningEnumerator()
        {
            //Arrange
            var sut = new LinkedList<object>();
            object e1 = new object(), e2 = new object(), e3 = new object();
            sut.Add(e1);
            sut.Add(e2);
            sut.Add(e3);
            bool containsE1 = false, containsE2 = false, containsE3 = false;

            //Act
            foreach (var @event in sut)
            {
                containsE1 |= @event == e1;
                containsE2 |= @event == e2;
                containsE3 |= @event == e3;
            }

            //Assert
            containsE1.Should().BeTrue();
            containsE2.Should().BeTrue();
            containsE3.Should().BeTrue();
        }
    }
}
