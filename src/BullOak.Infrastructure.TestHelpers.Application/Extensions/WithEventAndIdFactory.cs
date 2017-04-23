namespace BullOak.Infrastructure.TestHelpers.Application.Extensions
{
    using System;
    using BullOak.Application;
    using BullOak.Common;
    using BullOak.EventStream;
    using BullOak.Messages;

    internal class WithEventAndIdFactory<TEvent, TSourceId> : IRequireOriginatingEntityType<TEvent, TSourceId>
        where TEvent : IParcelVisionEvent
        where TSourceId : IId, IEquatable<TSourceId>
    {
        private TEvent Event { get; set; }
        private TSourceId SourceId { get; set; }

        public WithEventAndIdFactory(TEvent @event, TSourceId sourceId)
        {
            Event = @event;
            SourceId = sourceId;
        }

        public ParcelVisionEventEnvelope FromAggregate<TSelf>()
            where TSelf : AggregateRoot<TSourceId>
        {
            return new ParcelVisionEventEnvelope<TSourceId, TSourceId, TEvent>(Event, SourceId, SourceId, typeof(TSelf));
        }

        public IRequireParentIdType<TEvent, TSourceId> FromChildEntity<TEntity>()
            where TEntity : Entity<TSourceId>
        {
            var aggregateType = typeof(AggregateRoot<TSourceId>);

            if (aggregateType.IsAssignableFrom(typeof(TEntity))) throw new Exception();

            return new WithSelfType<TEvent, TSourceId>(Event, SourceId, typeof(TEntity));
        }
    }
}