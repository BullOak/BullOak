namespace BullOak.Application.Test.Unit
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class RetryTests
    {

        [Fact]
        public async Task RetryOnException_NoExceptionInCall_NoRetries()
        {
            // arrange
            int RetryLimit = 5;
            TimeSpan RetryMinInterval = TimeSpan.FromMilliseconds(20);
            TimeSpan RetryMaxInterval = TimeSpan.FromMilliseconds(200);
            TimeSpan RetryDelta = TimeSpan.FromMilliseconds(50);

            var a = new TestClass();
            int callCount = 0;
            Func<TestClass, Task> call = (x) => { callCount++; return Task.FromResult(x); };

            // Act
            await Retry.RetryOnException<TestClass, Exception>(
                call, a,
                RetryLimit, RetryMinInterval, RetryMaxInterval, RetryDelta,
                Retry.LinearIncreaseDelayPolicy);

            // Assert
            callCount.Should().Be(1);
        }

        [Fact]
        public async Task RetryOnException_OneExceptionInCall_RetriesOnce()
        {
            // arrange
            int RetryLimit = 3;
            TimeSpan RetryMinInterval = TimeSpan.FromMilliseconds(10);
            TimeSpan RetryMaxInterval = TimeSpan.FromMilliseconds(100);
            TimeSpan RetryDelta = TimeSpan.FromMilliseconds(5);

            var a = new TestClass();
            int callCount = 0;
            Func<TestClass, Task> call = (x) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new TestClassException($"Failed on iteration {callCount}");
                }

                return Task.FromResult(x);
            };

            // Act
            await Retry.RetryOnException<TestClass, TestClassException>(
                call, a,
                RetryLimit, RetryMinInterval, RetryMaxInterval, RetryDelta,
                Retry.LinearIncreaseDelayPolicy);

            // Assert
            callCount.Should().Be(2);
        }

        [Fact]
        public async Task RetryOnException_ExceptionInCall_RetriesUntilRetryLimitExpires()
        {
            // arrange
            int RetryLimit = 3;
            TimeSpan RetryMinInterval = TimeSpan.FromMilliseconds(10);
            TimeSpan RetryMaxInterval = TimeSpan.FromMilliseconds(100);
            TimeSpan RetryDelta = TimeSpan.FromMilliseconds(30);

            var a = new TestClass();
            int callCount = 0;
            Func<TestClass, Task> call = (x) =>
            {
                callCount++;
                throw new TestClassException($"Failed on iteration {callCount}");
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await Retry.RetryOnException<TestClass, TestClassException>(
                    call, a,
                    RetryLimit, RetryMinInterval, RetryMaxInterval, RetryDelta,
                    Retry.LinearIncreaseDelayPolicy));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<TestClassException>();
            callCount.Should().Be(RetryLimit + 1);
        }

        [Fact]
        public async Task RetryOnException_UnexpectedExceptionInCall_DoesNotRetry()
        {
            // arrange
            int RetryLimit = 3;
            TimeSpan RetryMinInterval = TimeSpan.FromMilliseconds(10);
            TimeSpan RetryMaxInterval = TimeSpan.FromMilliseconds(100);
            TimeSpan RetryDelta = TimeSpan.FromMilliseconds(5);

            var a = new TestClass();
            int callCount = 0;
            Func<TestClass, Task> call = (x) =>
            {
                callCount++;
                throw new Exception($"Failed on iteration {callCount}");
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await Retry.RetryOnException<TestClass, TestClassException>(
                    call, a,
                    RetryLimit, RetryMinInterval, RetryMaxInterval, RetryDelta,
                    Retry.LinearIncreaseDelayPolicy));


            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<Exception>();
            callCount.Should().Be(1);
        }

        public class TestClass
        { }

        public class TestClassException : Exception
        {
            public TestClassException(string message) : base(message)
            { }
        }
    }
}


