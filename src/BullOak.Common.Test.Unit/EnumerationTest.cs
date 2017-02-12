namespace BullOak.Common.Test.Unit
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class EnumerationLookUp : BullOak.Common.Enumeration<EnumerationLookUp, int>
    {
        public static readonly EnumerationLookUp EnumStatus1 = new EnumStatus1Type();
        public static readonly EnumerationLookUp EnumStatus2 = new EnumStatus2Type();
        public static readonly EnumerationLookUp EnumStatus3 = new EnumStatus3Type();

        public EnumerationLookUp(int value, string displayName) : base(value, displayName)
        {
        }

        private class EnumStatus1Type : EnumerationLookUp
        {
            public EnumStatus1Type() : base(0, "EnumStatus1")
            {
            }
        }

        private class EnumStatus2Type : EnumerationLookUp
        {
            public EnumStatus2Type() : base(1, "EnumStatus2")
            {
            }
        }

        private class EnumStatus3Type : EnumerationLookUp
        {
            public EnumStatus3Type() : base(3, "EnumStatus3")
            {
            }
        }
    }

    public class EnumerationTest
    {
        [Fact]
        public void Parse_WhenGetsValidEnumerationName_ReturnsTheCorrectValue()
        {
            // Act
            var result = EnumerationLookUp.Parse("EnumStatus1");

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(EnumerationLookUp.EnumStatus1);
        }

        [Fact]
        public void Parse_WhenGetsInValidEnumerationName_ThrowsException()
        {
            // Act
            var exception = Record.Exception(() => EnumerationLookUp.Parse("EnumStatus5"));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentException>();
        }

        [Fact]
        public void GetAll_WhenCalled_ReturnsValidEnumerations()
        {
            // Act 
            var enumList = EnumerationLookUp.GetAll();

            // Assert 
            enumList.Length.Should().Be(3);
            enumList[0].DisplayName.Should().Be(EnumerationLookUp.EnumStatus1.ToString());
            enumList[1].DisplayName.Should().Be(EnumerationLookUp.EnumStatus2.ToString());
            enumList[2].DisplayName.Should().Be(EnumerationLookUp.EnumStatus3.ToString());
        }

        [Fact]
        public void ToString_ValidEnumTypeProvided_ReturnsDisplayName()
        {
            // Act
            var name = EnumerationLookUp.EnumStatus1.ToString();

            // Assert
            name.Should().Be("EnumStatus1");

        }
    }
}
