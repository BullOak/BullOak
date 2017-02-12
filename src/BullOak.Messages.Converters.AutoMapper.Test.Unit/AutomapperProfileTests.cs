namespace Willow.Events.Upconverters.Tests.Unit
{
    using System;
    using AutoMapper;
    using BullOak.Messages;
    using BullOak.Messages.Converters;
    using BullOak.Messages.Converters.AutoMapper.Exceptions;
    using BullOak.Messages.Converters.AutoMapper.PreferenceAttributes;
    using FluentAssertions;
    using Moq;
    using Xunit;

    public class AutomapperProfileTests
    {
        public class Event_V1 : ParcelVisionEvent
        {
            public Event_V1() : base(Guid.Empty) { }
            public Event_V1(Guid correlationId) : base(correlationId)
            { }
        }

        public class Event_V2 : ParcelVisionEvent
        {
            public Event_V2() : base(Guid.Empty) { }
            public Event_V2(Guid correlationId) : base(correlationId)
            { }
        }

        private class UpconverterWithoutAutomapperAttribute : DefaultConverter<Event_V1, Event_V2>
        { }

        [DoNotUseAutomapper]
        [AutomaticallyCreateDefaultMappingsFromConverterGenericTypes]
        private class UpconverterWithMultipleAutomapperAttributes : DefaultConverter<Event_V1, Event_V2>
        { }

        [DoNotUseAutomapper]
        private class UpconverterWithIgnoreAutomapperAttribute : DefaultConverter<Event_V1, Event_V2>
        { }

        [DoNotUseAutomapper]
        private class UpconverterWithIgnoreAutomapperPlustRegistrationMethodAttribute : DefaultConverter<Event_V1, Event_V2>
        {
            public static void RegisterMapper(IProfileExpression config)
            {
                throw new Exception("Should not be called");
            }
        }

        [AutomaticallyCreateDefaultMappingsFromConverterGenericTypes]
        private class UpconverterWithAutomaticAutomapperRegistrationAttribute : DefaultConverter<Event_V1, Event_V2>
        { }

        [AutomaticallyCreateDefaultMappingsFromConverterGenericTypes]
        private class UpconverterWithAutomaticAutomapperRegistrationPlusRegistrationMethodAttribute :
            DefaultConverter<Event_V1, Event_V2>
        {
            public static void RegisterMapper(IProfileExpression config)
            {
                throw new Exception("Should not be called");
            }
        }

        [ThrowExceptionIfAutomapperRegistrationDoesNotExist]
        private class UpconverterWithRequireAutomapperRegistrationAttribute : DefaultConverter<Event_V1, Event_V2>
        {
            public static void RegisterMapper(IProfileExpression config)
            {
                config.CreateMap<Event_V1, Event_V2>();
            }
        }

        [ThrowExceptionIfAutomapperRegistrationDoesNotExist]
        private class UpconverterWithRequireAutomapperRegistrationButMissingMethodAttribute : DefaultConverter<Event_V1, Event_V2>
        { }

        private class DefaultConverter<TSource, TDestination> : EventConverterBase<TSource, TDestination>
            where TSource : ParcelVisionEvent
            where TDestination : ParcelVisionEvent, new()
        {
            public string PropertyValueToUse { get; }

            public DefaultConverter(string propertyOverride = null)
            {
                PropertyValueToUse = propertyOverride;
            }

            public override TDestination Convert(TSource @event)
            {
                return new TDestination()
                {
                    CorrelationId = @event.CorrelationId,
                };
            }
        }

        [Fact]
        public void Ctor_WithNoConverters_ShouldThrowException()
        {
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(new IEventConverter[0]));

            exception.Should().BeNull();
        }


        [Fact]
        public void Ctor_WithNullConverterList_ShouldThrowException()
        {
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(null));

            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_WithEmptyConverterList_ShouldNotThrowException()
        {
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(new IEventConverter[0]));

            exception.Should().BeNull();
        }

        [Fact]
        public void Ctor_UpconverterDoesNotHaveAutomapperRegistrationAttribute_ShouldThrow()
        {
            var upconverter = new UpconverterWithoutAutomapperAttribute();
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(upconverter));

            exception.Should().NotBeNull();
            exception.Should().BeOfType<AutomapperRegistrationPreferenceMissingException>();
        }

        [Fact]
        public void Ctor_UpconverterHasMoreThanOneAutomapperRegistrastionAttributes_ShouldThrow()
        {
            var upconverter = new UpconverterWithMultipleAutomapperAttributes();
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(upconverter));

            exception.Should().NotBeNull();
            exception.Should().BeOfType<MultipleAutomapperRegistrationPreferencesDetectedException>();
        }

        [Fact]
        public void Ctor_UpconverterWithDoNotUseAutomapperAttribute_ShouldNotThrow()
        {
            var upconverter = new UpconverterWithIgnoreAutomapperAttribute();
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(upconverter));

            exception.Should().BeNull();
        }

        [Fact]
        public void Ctor_UpconverterWithDoNotUseAutomapperAttributeButRegistrationMethod_ShouldNotThrow()
        {
            var upconverter = new UpconverterWithIgnoreAutomapperPlustRegistrationMethodAttribute();
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(upconverter));

            exception.Should().BeNull();
        }

        [Fact]
        //NOTE: This is implicitly checking that it is not throwing. I am open to discussion about not having an additional check for non-exception
        // (although my pov is that it is redundant)
        public void Ctor_UpconverterWithAutomaticallyCreateMappingAttribute_ShouldCreateMap()
        {
            var upconverter = new UpconverterWithAutomaticAutomapperRegistrationAttribute();
            var config = new Mock<IMapperConfigurationExpression>();

            config.Object.ValidateConvertersAndRegisterMaps(upconverter);

            config.Verify(x => x.CreateMap(typeof(Event_V1), typeof(Event_V2)), Times.Once);
        }

        [Fact]
        public void Ctor_UpconverterWithAutomaticallyCreateMappingAttributeButRegistrationMethod_ShouldNotThrow()
        {
            var upconverter = new UpconverterWithAutomaticAutomapperRegistrationPlusRegistrationMethodAttribute();
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(upconverter));

            exception.Should().BeNull();
        }

        [Fact]
        public void Ctor_UpconverterWithRequireAutomapperRegistrationAttribute_ShouldNotThrow()
        {
            var upconverter = new UpconverterWithRequireAutomapperRegistrationAttribute();
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(upconverter));

            exception.Should().BeNull();
        }

        [Fact]
        public void Ctor_UpconverterWithRequireAutomapperRegistrationButNoMethodAttribute_ShouldThrow()
        {
            var upconverter = new UpconverterWithRequireAutomapperRegistrationButMissingMethodAttribute();
            var config = new Mock<IMapperConfigurationExpression>();

            var exception = Record.Exception(() => config.Object.ValidateConvertersAndRegisterMaps(upconverter));

            exception.Should().NotBeNull();
            exception.Should().BeOfType<AutomapperRegistrationRequiredException>();
        }
    }
}
