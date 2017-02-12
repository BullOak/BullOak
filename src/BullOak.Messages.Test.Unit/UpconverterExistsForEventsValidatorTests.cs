namespace BullOak.Messages.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using Converters;
    using FluentAssertions;
    using Xunit;

    public class UpconverterExistsForEventsValidatorTests
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

        public class Event_V3 : ParcelVisionEvent
        {
            public Event_V3() : base(Guid.Empty) { }
            public Event_V3(Guid correlationId) : base(correlationId)
            { }
        }

        private class DefaultConverter<TSource, TDestination> : EventConverterBase<TSource, TDestination>
            where TSource : ParcelVisionEvent
            where TDestination : ParcelVisionEvent, new()
        {
            public string PropertyValutToUse { get; }

            public DefaultConverter(string propertyOverride = null)
            {
                PropertyValutToUse = propertyOverride;
            }

            public override TDestination Convert(TSource @event)
            {
                return new TDestination()
                {
                    CorrelationId = @event.CorrelationId,
                };
            }
        }

        private class Arrangements
        {
            public IEnumerable<Type> EventTypes { get; }
            private IList<IEventConverter> converters;

            public IEnumerable<IEventConverter> Converters
            { get { return converters; } }

            private Type openGenericTypeOfDefaultConverter;

            public Arrangements()
            {
                EventTypes = new[] {typeof(Event_V1), typeof(Event_V2), typeof(Event_V3)};
                converters = new List<IEventConverter>();
                openGenericTypeOfDefaultConverter = typeof(DefaultConverter<,>);
            }

            public void AddUpconverter<TSource, TDestination>()
                where TSource : ParcelVisionEvent
                where TDestination : ParcelVisionEvent, new()
            {
                converters.Add(new DefaultConverter<TSource, TDestination>());
            }
        }

        [Fact]
        public void CheckIfUpconvertersExistOrThrow_WithThreEventsAndAllUpconverters_ShouldNotThrow()
        {
            var arrangements = new Arrangements();
            arrangements.AddUpconverter<Event_V1, Event_V2>();
            arrangements.AddUpconverter<Event_V2, Event_V3>();

            var exception = Record.Exception(() => UpconverterExistsForEventsValidator.CheckIfUpconvertersExistOrThrow(arrangements.EventTypes, t => t == typeof(Event_V1), arrangements.Converters));

            exception.Should().BeNull();
        }

        [Fact]
        public void CheckIfUpconvertersExistOrThrow_WithThreEventsWithOnlyOneUpconverter_ShouldThrow()
        {
            var arrangements = new Arrangements();
            arrangements.AddUpconverter<Event_V1, Event_V2>();

            var exception = Record.Exception(() => UpconverterExistsForEventsValidator.CheckIfUpconvertersExistOrThrow(arrangements.EventTypes, t => t == typeof(Event_V1), arrangements.Converters));

            exception.Should().NotBeNull();
            exception.Should().BeOfType<MissingUpconverterException>();
        }

        [Fact]
        public void CheckIfUpconvertersExistOrThrow_WithThreEventsAndAllUpconvertersButWrongOriginChecker_ShouldThrow()
        {
            var arrangements = new Arrangements();
            arrangements.AddUpconverter<Event_V1, Event_V2>();
            arrangements.AddUpconverter<Event_V2, Event_V3>();

            var exception = Record.Exception(() => UpconverterExistsForEventsValidator.CheckIfUpconvertersExistOrThrow(arrangements.EventTypes, t => t == typeof(Event_V3), arrangements.Converters));

            exception.Should().NotBeNull();
            exception.Should().BeOfType<MissingUpconverterException>();
        }

        [Fact]
        public void CheckIfUpconvertersExistOrThrow_WithThreEventsAndAllUpconvertersRegisteredTwice_ShouldNotThrow()
        {
            var arrangements = new Arrangements();
            arrangements.AddUpconverter<Event_V1, Event_V2>();
            arrangements.AddUpconverter<Event_V1, Event_V2>();
            arrangements.AddUpconverter<Event_V2, Event_V3>();
            arrangements.AddUpconverter<Event_V2, Event_V3>();

            var exception = Record.Exception(() => UpconverterExistsForEventsValidator.CheckIfUpconvertersExistOrThrow(arrangements.EventTypes, t => t == typeof(Event_V1), arrangements.Converters));

            exception.Should().BeNull();
        }
    }
}
