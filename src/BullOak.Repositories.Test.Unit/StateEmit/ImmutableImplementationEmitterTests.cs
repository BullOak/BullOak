namespace BullOak.Repositories.Test.Unit.StateEmit
{
    using System;
    using BullOak.Repositories.StateEmit;
    using BullOak.Repositories.StateEmit.Emitters;
    using FluentAssertions;
    using Xunit;

    public interface TestInterface : ValueType<TestInterface>
    {
        int Value { get; }
        string Name { get; }
    }

    public class ImmutableImplementationEmitterTests
    {
        private EmittedTypeFactory sut => EmittedTypeFactory.Instance;

        [Fact]
        public void GetState_ShouldReturnAValueType()
        {
            object result = null;

            var exception = Record.Exception(() =>
            {
                result = sut.GetState<TestInterface>();
            });

            result.Should().BeAssignableTo<TestInterface>();
            result.Should().BeAssignableTo<ValueType<TestInterface>>();
        }

        [Fact]
        public void With_CreatesAShallowCopy()
        {
            var original = sut.GetState<TestInterface>();
            var copyWithValue = original.With(t => t.Value, 5);

            original.Should().NotBeSameAs(copyWithValue);
            Assert.False(original == copyWithValue);
        }

        [Fact]
        public void With_CreatesACopyWithNewSpecifiedValue()
        {
            var original = sut.GetState<TestInterface>();
            var expectedValue = 3;
            var expectedName = "newName";

            var copyWithValue = original.With(t => t.Value, expectedValue)
                .With(t => t.Name, expectedName);

            copyWithValue.Name.Should().NotBe(original.Name);
            copyWithValue.Name.Should().Be(expectedName);

            copyWithValue.Value.Should().NotBe(original.Value);
            copyWithValue.Value.Should().Be(expectedValue);
        }
    }
}
