namespace BullOak.Infrastructure.TestHelpers.Application.Extensions
{
    using System;
    using BullOak.Common;
    using BullOak.EventStream;
    using BullOak.Messages;

    internal class WithSelfType<TEvent, TSourceId> : IRequireParentIdType<TEvent, TSourceId>
        where TEvent : IParcelVisionEvent
        where TSourceId : IId
    {
        private TEvent Event { get; set; }
        private TSourceId SourceId { get; set; }
        private Type SourceEntityType { get; set; }

        public WithSelfType(TEvent @event, TSourceId sourceId, Type sourceType)
        {
            Event = @event;
            SourceId = sourceId;
            SourceEntityType = sourceType;
        }

        ParcelVisionEventEnvelope IRequireParentIdType<TEvent, TSourceId>.WithParentId<TParentId>(TParentId parentId)
        {
            return new ParcelVisionEventEnvelope<TSourceId, TParentId, TEvent>(Event, SourceId, parentId,
                SourceEntityType);
        }
    }
}