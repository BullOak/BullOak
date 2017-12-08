namespace BullOak.Repositories.Test.Unit.StateEmit
{
    using System.Linq;
    using BullOak.Repositories.StateEmit;
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

        [Fact]
        public void EmitType_OfInterfaceWithOnlyProperties_ShouldReturnTypeOfClass()
        {
            //Arrange

            //Act
            var myType = StateTypeEmitter.EmitType(typeof(MyInterface));

            //Assert
            myType.IsClass.Should().BeTrue();
        }

        [Fact]
        public void EmitType_OfInterfaceWithOnlyProperties_ShouldReturnTypeThatImplementsInterface()
        {
            //Arrange
            var myType = StateTypeEmitter.EmitType(typeof(MyInterface));

            //Act
            bool implementsMyInterface = myType.GetInterfaces().Any(x => x.Name == typeof(MyInterface).Name);

            //Assert
            implementsMyInterface.Should().BeTrue();
        }

        [Fact]
        public void EmitType_OfInterfaceWithOnlyProperties_ShouldReturnTypeThatImplementsICanSwitchBackAndToReadOnly()
        {
            //Arrange
            var myType = StateTypeEmitter.EmitType(typeof(MyInterface));

            //Act
            bool implementsMyInterface = myType.GetInterfaces().Any(x => x.Name == typeof(ICanSwitchBackAndToReadOnly).Name);

            //Assert
            implementsMyInterface.Should().BeTrue();
        }
    }
}
