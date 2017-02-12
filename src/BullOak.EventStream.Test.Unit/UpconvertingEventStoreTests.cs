namespace BullOak.EventStream.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Common;
    using FluentAssertions;
    using Messages;
    using Messages.Converters;
    using Upconvert;
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

        private class EventStore : IEventStore
        {
            public readonly ParcelVisionEventEnvelope[] eventsToServe;
            public EventStore(params ParcelVisionEventEnvelope[] eventsToServe)
            {
                this.eventsToServe = eventsToServe;
            }

            public Task<bool> Exists(string id)
            {
                var exists = this.eventsToServe.ToList().Any(x => x.Event.Id.ToString() == id);
                return Task.FromResult(exists);
            }

            public Task<EventStoreData> LoadFor(string id)
            {
                return Task.FromResult(new EventStoreData(eventsToServe, 0));
            }

            public Task Store(string id, int concurrencyData, IEnumerable<IParcelVisionEventEnvelope> newEvents)
            {
                return Task.Delay(0);
            }
        }

        private class DefaultConverterBase<TSource, TDestination> : EventConverterBase<TSource, TDestination>
            where TSource : MyBaseEvent
            where TDestination : MyBaseEvent, new()
        {
            public override TDestination Convert(TSource @event)
            {
                return new TDestination()
                {
                    CorrelationId = @event.CorrelationId,
                    MyProperty = @event.MyProperty,
                };
            }
        }

        private class Arrangements
        {
            public MyEvent1 OriginalEvent => OriginalEventStore.eventsToServe.FirstOrDefault().Event as MyEvent1;

            public EventStore OriginalEventStore { get; } = new EventStore(

                new ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>
                {
                    SourceId = (EntityId)"SourceId",
                    ParentId = (EntityId)"ParentId",
                    EventRaw = new MyEvent1(),
                    SourceEntityType = typeof(Arrangements)
                }
            );

            public UpconvertingEventStore SUT { get; set; }

            public Arrangements(params IEventConverter[] converters)
            {
                SUT = new UpconvertingEventStore(OriginalEventStore, converters);
            }
        }

        private Arrangements GetArrangements(params IEventConverter[] converters)
        {
            return new Arrangements(converters);
        }

        [Fact]
        public async Task GetUpconvertedEvents_WithNoUpconverters_ShouldReturnOriginalEvents()
        {
            var arrangements = GetArrangements();

            var events = await arrangements.SUT.LoadFor("");

            events.EventEnvelopes.Count().Should().Be(1);
            events.EventEnvelopes.FirstOrDefault()?.Event.Should().BeOfType<MyEvent1>();
            events.EventEnvelopes.FirstOrDefault().As<ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>>()?.SourceId.Should().Be((EntityId)"SourceId");
            events.EventEnvelopes.FirstOrDefault().As<ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>>()?.ParentId.Should().Be((EntityId)"ParentId");
            events.EventEnvelopes.FirstOrDefault()?.SourceEntityType.Should().Be(typeof(Arrangements));
        }

        [Fact]
        public async Task GetUpconvertedEvents_WithOneEventWithOneUpconverter_ShouldReturnUpconvertedEvent()
        {
            var arrangements = GetArrangements(new DefaultConverterBase<MyEvent1, MyEvent2>());

            var events = await arrangements.SUT.LoadFor("");

            events.EventEnvelopes.Count().Should().Be(1);
            events.EventEnvelopes.FirstOrDefault()?.Event.Should().BeOfType<MyEvent2>();
            events.EventEnvelopes.FirstOrDefault()?.Event.As<MyEvent2>().MyProperty.Should().Be(arrangements.OriginalEvent.MyProperty);
            events.EventEnvelopes.FirstOrDefault()?.Event.CorrelationId.Should().Be(arrangements.OriginalEvent.CorrelationId);
            events.EventEnvelopes.FirstOrDefault().As<ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>>()?.SourceId.Should().Be((EntityId)"SourceId");
            events.EventEnvelopes.FirstOrDefault().As<ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>>()?.ParentId.Should().Be((EntityId)"ParentId");
            events.EventEnvelopes.FirstOrDefault()?.SourceEntityType.Should().Be(typeof(Arrangements));
        }

        [Fact]
        public async Task GetUpconvertedEvents_WithOneEventWithTwoSequentialUpconverters_ShouldUpconvertToFinalType()
        {
            var arrangements = GetArrangements(new DefaultConverterBase<MyEvent1, MyEvent2>(), new DefaultConverterBase<MyEvent2, MyEvent3>());

            var events = await arrangements.SUT.LoadFor("");

            events.EventEnvelopes.Count().Should().Be(1);
            events.EventEnvelopes.FirstOrDefault()?.Event.Should().BeOfType<MyEvent3>();
            events.EventEnvelopes.FirstOrDefault()?.Event.As<MyEvent3>().MyProperty.Should().Be(arrangements.OriginalEvent.MyProperty);
            events.EventEnvelopes.FirstOrDefault()?.Event.CorrelationId.Should().Be(arrangements.OriginalEvent.CorrelationId);
            events.EventEnvelopes.FirstOrDefault().As<ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>>()?.SourceId.Should().Be((EntityId)"SourceId");
            events.EventEnvelopes.FirstOrDefault().As<ParcelVisionEventEnvelope<EntityId, EntityId, MyEvent1>>()?.ParentId.Should().Be((EntityId)"ParentId");
            events.EventEnvelopes.FirstOrDefault()?.SourceEntityType.Should().Be(typeof(Arrangements));
        }

        [Fact]
        public async Task GetUpconvertedEvents_WithOneEventWithOneUpconverterThatCanThenFeedIntoTwoOtherUpconverters_ShouldUpconvertToTwoEvents()
        {
            var arrangements = GetArrangements(new DefaultConverterBase<MyEvent1, MyEvent2>(), new DefaultConverterBase<MyEvent2, MyEvent3>(), new DefaultConverterBase<MyEvent2, MyEvent4>());

            var events = await arrangements.SUT.LoadFor("");

            events.EventEnvelopes.Count().Should().Be(2);
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent3).Should().NotBeNull();
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent3)?.Event.As<MyEvent3>().MyProperty.Should().Be(arrangements.OriginalEvent.MyProperty);
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent3)?.Event.CorrelationId.Should().Be(arrangements.OriginalEvent.CorrelationId);
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent4).Should().NotBeNull();
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent4)?.Event.As<MyEvent4>().MyProperty.Should().Be(arrangements.OriginalEvent.MyProperty);
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent4)?.Event.CorrelationId.Should().Be(arrangements.OriginalEvent.CorrelationId);
        }

        [Fact]
        public async Task GetUpconvertedEvents_WithOneEventWithConvererTree_ShouldUpconvertToAllLeafEvents()
        {
            var arrangements = GetArrangements(
                new DefaultConverterBase<MyEvent1, MyEvent2>(),
                new DefaultConverterBase<MyEvent1, MyEvent3>(),
                new DefaultConverterBase<MyEvent2, MyEvent4>(),
                new DefaultConverterBase<MyEvent4, MyEvent4_1>(),
                new DefaultConverterBase<MyEvent2, MyEvent5>(),
                new DefaultConverterBase<MyEvent3, MyEvent6>(),
                new DefaultConverterBase<MyEvent3, MyEvent7>()
                );

            var events = await arrangements.SUT.LoadFor("");

            events.EventEnvelopes.Count().Should().Be(4);
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent4_1).Should().NotBeNull();
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent5).Should().NotBeNull();
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent6).Should().NotBeNull();
            events.EventEnvelopes.FirstOrDefault(x => x.Event is MyEvent7).Should().NotBeNull();
        }
    }
}
