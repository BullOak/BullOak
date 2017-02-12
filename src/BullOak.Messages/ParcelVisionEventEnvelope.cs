namespace BullOak.Messages
{
    using System;

    public abstract class ParcelVisionEventEnvelope : IParcelVisionEventEnvelope
    {
        public Type SourceEntityType { get; set; }

        public abstract IParcelVisionEvent Event { get; }

        public abstract ParcelVisionEventEnvelope CloneWithEvent<TEvent>(TEvent @event)
            where TEvent: IParcelVisionEvent;
    }

    public abstract class ParcelVisionEventEnvelope<TSourceEntityId> : ParcelVisionEventEnvelope
    {
        public TSourceEntityId SourceId { get; set; }
    }

    public sealed class ParcelVisionEventEnvelope<TSourceEntityId, TParentId, TEvent> : ParcelVisionEventEnvelope<TSourceEntityId>, 
        IParcelVisionEventEnvelope<TSourceEntityId, TParentId>, 
        IHaveSourceAndParentIdTypes
        where TEvent : IParcelVisionEvent
    {
        public TParentId ParentId { get; set; }
        public TEvent EventRaw { get; set; }
        public override IParcelVisionEvent Event => EventRaw;

        public Type SourceIdType => SourceId.GetType();
        public Type ParentIdType => ParentId.GetType();

        public ParcelVisionEventEnvelope()
        { }

        public ParcelVisionEventEnvelope(TEvent @event, TSourceEntityId sourceId, TParentId parentId, Type sourceType)
        {
            EventRaw = @event;
            SourceId = sourceId;
            ParentId = parentId;
            SourceEntityType = sourceType;
        }

        public override ParcelVisionEventEnvelope CloneWithEvent<TNewEvent>(TNewEvent @event)
        {
            return new ParcelVisionEventEnvelope<TSourceEntityId, TParentId, TNewEvent>()
            {
                EventRaw = @event,
                ParentId = ParentId,
                SourceEntityType = SourceEntityType,
                SourceId = SourceId
            };
        }
    }
}