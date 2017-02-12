namespace BullOak.Messages.Test.Unit
{
    using System;
    using System.Linq;
    using BullOak.Common;
    using Converters;
    using FluentAssertions;
    using Xunit;

    public class UpconvertingEventStoreTests
    {
        private class MyBaseEvent : ParcelVisionEvent
        {
            public string MyProperty { get; set; }

            public MyBaseEvent()
                : base(Guid.NewGuid())
            {
                MyProperty = Guid.NewGuid().ToString();
            }
        }

        private struct EntityId : IId
        { 
            public string Id { get; private set; }

            public static implicit operator string(EntityId id)
            {
                return id.Id;
            }

            public static explicit operator EntityId(string id)
            {
                var entityId = new EntityId { Id = id };

                return entityId;
            }

            public override string ToString()
            {
                return Id;
            }
        }

        private class MyEvent1 : MyBaseEvent { }
        private class MyEvent2 : MyBaseEvent { }
        private class MyEvent3 : MyBaseEvent { }
        private class MyEvent4 : MyBaseEvent { }
        private class MyEvent4_1 : MyBaseEvent { }
        private class MyEvent5 : MyBaseEvent { }
        private class MyEvent6 : MyBaseEvent { }
        private class MyEvent7 : MyBaseEvent { }

        private class DefaultConverter<TSource, TDestination> : EventConverterBase<TSource, TDestination>
            where TSource : MyBaseEvent
            where TDestination : MyBaseEvent, new()
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
                    MyProperty = PropertyValutToUse ?? @event.MyProperty,
                };
            }
        }

        private class Arrangements
        {
            public ParcelVisionEventEnvelope OriginalEvent { get; } = new ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>
            {
                SourceId = (EntityId)"SourceId",
                ParentId = (EntityId)"ParentId",
                EventRaw = new MyEvent1()
                { 
                    MyProperty = "MyOriginalEventProperty"
                },
                SourceEntityType = typeof(Arrangements)
            };

            public RecursiveEventUpconverter SUT { get; }

            public Arrangements(params IEventConverter[] converters)
            {
                SUT = new RecursiveEventUpconverter(converters);
            }
        }

        private Arrangements GetArrangements(params IEventConverter[] converters)
        {
            return new Arrangements(converters);
        }

        [Fact]
        public void GetUpconvertedEvents_WithNoUpconverters_ShouldReturnOriginalEvents()
        {
            var arrangements = GetArrangements();

            var events = arrangements.SUT.UpconvertEvent(arrangements.OriginalEvent);

            events.Count().Should().Be(1);
            events.FirstOrDefault().Event.Should().BeOfType<MyEvent1>();
            events.FirstOrDefault().As<ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>>().SourceId.Should().Be((EntityId) "SourceId");
            events.FirstOrDefault().As<ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>>().ParentId.Should().Be((EntityId) "ParentId");
            events.FirstOrDefault().SourceEntityType.Should().Be(typeof(Arrangements));
        }

        [Fact]
        public void GetUpconvertedEvents_WithOneEventWithOneUpconverter_ShouldReturnUpconvertedEvent()
        {
            var arrangements = GetArrangements(new DefaultConverter<MyEvent1, MyEvent2>());

            var events = arrangements.SUT.UpconvertEvent(arrangements.OriginalEvent);

            events.Count().Should().Be(1);
            events.FirstOrDefault().Event.Should().BeOfType<MyEvent2>();
            events.FirstOrDefault().Event.As<MyEvent2>().MyProperty.Should().Be(((MyEvent1)arrangements.OriginalEvent.Event).MyProperty);
            events.FirstOrDefault().Event.CorrelationId.Should().Be(arrangements.OriginalEvent.Event.CorrelationId);
            events.FirstOrDefault().As<IParcelVisionEventEnvelope<EntityId, EntityId>>().SourceId.Should().Be((EntityId)"SourceId");
            events.FirstOrDefault().As<IParcelVisionEventEnvelope<EntityId, EntityId>>().ParentId.Should().Be((EntityId)"ParentId");
            events.FirstOrDefault().SourceEntityType.Should().Be(typeof(Arrangements));
        }

        [Fact]
        public void GetUpconvertedEvents_WithOneEventWithTwoSequentialUpconverters_ShouldUpconvertToFinalType()
        {
            var arrangements = GetArrangements(new DefaultConverter<MyEvent1, MyEvent2>(), new DefaultConverter<MyEvent2, MyEvent3>());

            var events = arrangements.SUT.UpconvertEvent(arrangements.OriginalEvent);

            events.Count().Should().Be(1);
            events.FirstOrDefault().Event.Should().BeOfType<MyEvent3>();
            events.FirstOrDefault().Event.As<MyEvent3>().MyProperty.Should().Be(((MyEvent1)arrangements.OriginalEvent.Event).MyProperty);
            events.FirstOrDefault().Event.CorrelationId.Should().Be(arrangements.OriginalEvent.Event.CorrelationId);
            events.FirstOrDefault().As<IParcelVisionEventEnvelope<EntityId, EntityId>>().SourceId.Should().Be((EntityId)"SourceId");
            events.FirstOrDefault().As<IParcelVisionEventEnvelope<EntityId, EntityId>>().ParentId.Should().Be((EntityId)"ParentId");
            events.FirstOrDefault().SourceEntityType.Should().Be(typeof(Arrangements));
        }

        [Fact]
        public void GetUpconvertedEvents_WithOneEventWithOneUpconverterThatCanThenFeedIntoTwoOtherUpconverters_ShouldUpconvertToTwoEvents()
        {
            var arrangements = GetArrangements(new DefaultConverter<MyEvent1, MyEvent2>(), new DefaultConverter<MyEvent2, MyEvent3>(), new DefaultConverter<MyEvent2, MyEvent4>());

            var events = arrangements.SUT.UpconvertEvent(arrangements.OriginalEvent);

            events.Count().Should().Be(2);
            events.FirstOrDefault(x => x.Event is MyEvent3).Should().NotBeNull();
            events.FirstOrDefault(x => x.Event is MyEvent3).Event.As<MyEvent3>().MyProperty.Should().Be(((MyEvent1)arrangements.OriginalEvent.Event).MyProperty);
            events.FirstOrDefault(x => x.Event is MyEvent3).Event.CorrelationId.Should().Be(arrangements.OriginalEvent.Event.CorrelationId);
            events.FirstOrDefault(x => x.Event is MyEvent4).Should().NotBeNull();
            events.FirstOrDefault(x => x.Event is MyEvent4).Event.As<MyEvent4>().MyProperty.Should().Be(((MyEvent1)arrangements.OriginalEvent.Event).MyProperty);
            events.FirstOrDefault(x => x.Event is MyEvent4).Event.CorrelationId.Should().Be(arrangements.OriginalEvent.Event.CorrelationId);
        }

        [Fact]
        public void GetUpconvertedEvents_WithOneEventWithConvererTree_ShouldUpconvertToAllLeafEvents()
        {
            var arrangements = GetArrangements(
                new DefaultConverter<MyEvent1, MyEvent2>(),
                new DefaultConverter<MyEvent1, MyEvent3>(),
                new DefaultConverter<MyEvent2, MyEvent4>(),
                new DefaultConverter<MyEvent4, MyEvent4_1>(),
                new DefaultConverter<MyEvent2, MyEvent5>(),
                new DefaultConverter<MyEvent3, MyEvent6>(),
                new DefaultConverter<MyEvent3, MyEvent7>()
                );

            var events = arrangements.SUT.UpconvertEvent(arrangements.OriginalEvent);

            events.Count().Should().Be(4);
            events.FirstOrDefault(x => x.Event is MyEvent4_1).Should().NotBeNull();
            events.FirstOrDefault(x => x.Event is MyEvent5).Should().NotBeNull();
            events.FirstOrDefault(x => x.Event is MyEvent6).Should().NotBeNull();
            events.FirstOrDefault(x => x.Event is MyEvent7).Should().NotBeNull();
        }

        [Fact()]
        public void GetUpconvertedEvents_WithOneEventWithTwoDuplicateConverters_ShouldUpconvertToASingleEvent()
        {
            var arrangements = GetArrangements(
                new DefaultConverter<MyEvent1, MyEvent2>(),
                new DefaultConverter<MyEvent1, MyEvent2>()
                );

            var events = arrangements.SUT.UpconvertEvent(arrangements.OriginalEvent);

            events.Count().Should().Be(1);
            events.FirstOrDefault(x => x.Event is MyEvent2).Should().NotBeNull();
            events.FirstOrDefault(x => x.Event is MyEvent2).Event.As<MyEvent2>().MyProperty.Should().Be(((MyEvent1)arrangements.OriginalEvent.Event).MyProperty);
            events.FirstOrDefault(x => x.Event is MyEvent2).Event.CorrelationId.Should().Be(arrangements.OriginalEvent.Event.CorrelationId);
        }

        [Fact()]
        public void GetUpconvertedEvents_WithOneEventWithThreeConvertersWhereFirstTwoInSeriesAreDuplicatingThirdConverter_ShouldUpconvertToASingleEventUsingLastConverter()
        {
            string propertyValue = "ShorterConverter";
            var arrangements = GetArrangements(
                new DefaultConverter<MyEvent1, MyEvent2>(),
                new DefaultConverter<MyEvent2, MyEvent3>(),
                new DefaultConverter<MyEvent1, MyEvent3>(propertyValue)
                );

            var events = arrangements.SUT.UpconvertEvent(arrangements.OriginalEvent);

            events.Count().Should().Be(1);
            events.FirstOrDefault(x => x.Event is MyEvent3).Should().NotBeNull();
            events.FirstOrDefault(x => x.Event is MyEvent3).Event.As<MyEvent3>().MyProperty.Should().Be(propertyValue);
            events.FirstOrDefault(x => x.Event is MyEvent3).Event.CorrelationId.Should().Be(arrangements.OriginalEvent.Event.CorrelationId);
        }
    }
}
