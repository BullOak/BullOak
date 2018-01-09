namespace BullOak.Repositories.Test.Unit.StateEmit
{
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.StateEmit;
    using BullOak.Repositories.StateEmit.Emitters;
    using FluentAssertions;
    using Xunit;

    public class StateTypeEmitterTests
    {
        public interface MyInterface
        {
            int MyValue { get; set; }
        }

        public interface IHavePropertiesAndMethods
        {
            int MyValue { get; set; }
            bool MyMethod();
        }

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
            bool implementsMyInterface = myType.GetInterfaces().Any(x => x.Name == typeof(ICanSwitchBackAndToReadOnly).Name);

            //Assert
            implementsMyInterface.Should().BeTrue();
        }
    }
}
