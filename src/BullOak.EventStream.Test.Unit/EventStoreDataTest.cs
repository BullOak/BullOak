

namespace BullOak.EventStream.Test.Unit
{
    using System;
    using Xunit;
    using System.Collections.Generic;
    using BullOak.Messages;

    public class EventStoreDataTest
    {
        // MethodName_StateUnderTest_ExpectedBehavior

        [Fact]
        public void EventStoreData_ConstructorParamEventsIsNull_ExpectException()
        {
            // Arrange
            // Act
            // Assert
            var ex = Assert.Throws<ArgumentException>(() => new EventStoreData(default(IEnumerable<ParcelVisionEventEnvelope>), 0));
            Assert.Equal("events", ex.Message);
        }
    }
}
