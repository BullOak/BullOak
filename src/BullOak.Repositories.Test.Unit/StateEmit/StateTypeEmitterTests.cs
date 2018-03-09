namespace BullOak.Repositories.Test.Unit.StateEmit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.StateEmit;
    using BullOak.Repositories.StateEmit.Emitters;
    using FluentAssertions;
    using Xunit;

    public class StateTypeEmitterTests
    {
        public static IEnumerable<object[]> GetEmitters()
        {
            yield return new[] {new StateWrapperEmitter()};
            yield return new[] {new OwnedStateClassEmitter()};
        }

        [Theory]
        [MemberData(nameof(GetEmitters))]
        public void EmitType_OfInterfaceWithOnlyProperties_ShouldReturnTypeOfClass(object emitter)
        {
            //Arrange

            //Act
            var myType = StateTypeEmitter.EmitType(typeof(MyInterface), emitter as BaseClassEmitter);

            //Assert
            myType.IsClass.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(GetEmitters))]
        public void EmitType_OfInterfaceWithOnlyProperties_ShouldReturnTypeThatImplementsInterface(object emitter)
        {
            //Arrange
            var myType = StateTypeEmitter.EmitType(typeof(MyInterface), emitter as BaseClassEmitter);

            //Act
            bool implementsMyInterface = myType.GetInterfaces().Any(x => x.Name == typeof(MyInterface).Name);

            //Assert
            implementsMyInterface.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(GetEmitters))]
        public void EmitType_OfInterfaceWithOnlyProperties_ShouldReturnTypeThatImplementsICanSwitchBackAndToReadOnly(object emitter)
        {
            //Arrange
            var myType = StateTypeEmitter.EmitType(typeof(MyInterface), emitter as BaseClassEmitter);

            //Act
            bool implementsMyInterface =
                myType.GetInterfaces().Any(x => x.Name == typeof(ICanSwitchBackAndToReadOnly).Name);

            //Assert
            implementsMyInterface.Should().BeTrue();
        }

        [Fact]
        public void EmitType_OfDerivedInterface_ShouldSucceed()
        {
            //Arrange
            var emitter = new OwnedStateClassEmitter();
            Type myType = null;

            //Act
            var exception = Record.Exception((Action) (() => myType = StateTypeEmitter.EmitType(typeof(MyDerivedOfNameAndSalary), emitter)));

            //Assert
            exception.Should().BeNull();
            myType.Should().NotBeNull();
            myType.Should().Implement(typeof(MyDerivedOfNameAndSalary));
            myType.Should().Implement(typeof(MyBaseWithNameAndSalary));
        }

        [Fact]
        public void EmitType_OfInterfaceDerivingFromTwoInterfaceWithSameNamedProperty_ShouldSucceed()
        {
            //Arrange
            var emitter = new OwnedStateClassEmitter();
            Type myType = null;

            //Act
            var exception = Record.Exception((Action) (() =>
                myType = StateTypeEmitter.EmitType(typeof(MyDerivedOfIntAndStringValues), emitter)));

            //Assert
            exception.Should().BeNull();
            myType.Should().NotBeNull();
            myType.Should().Implement(typeof(MyDerivedOfIntAndStringValues));
            myType.Should().Implement(typeof(MyBaseWithStringValue));
            myType.Should().Implement(typeof(MyBaseWithIntValue));
        }

        [Fact]
        public void EmitType_OfInterfaceDerivingFromTwoInterfaceWithSameNamedButDifferntTypeProperty_ShouldImplementPropertiesExplicitly()
        {
            //Arrange
            var emitter = new OwnedStateClassEmitter();
            Type myType = null;

            //Act
            var exception = Record.Exception((Action) (() => myType = StateTypeEmitter.EmitType(typeof(MyDerivedOfIntAndStringValues), emitter)));

            //Assert
            exception.Should().BeNull();
            myType.Should().NotBeNull();
            myType.Should().Implement(typeof(MyDerivedOfIntAndStringValues));
            myType.Should().Implement(typeof(MyBaseWithStringValue));
            myType.Should().Implement(typeof(MyBaseWithIntValue));
        }
    }
}
