namespace BullOak.Messages
{
    using System;

    public interface IParcelVisionEventEnvelope
    {
        IParcelVisionEvent Event { get; }
        Type SourceEntityType { get; set; }
        ParcelVisionEventEnvelope CloneWithEvent<TEvent>(TEvent @event) where TEvent : IParcelVisionEvent;
    }

    public interface IParcelVisionEventEnvelope<out TSourceEntityId> : IParcelVisionEventEnvelope
    {
        TSourceEntityId SourceId { get; }
    }

    public interface IParcelVisionEventEnvelope<out TSourceEntityId, out TParentId> : IParcelVisionEventEnvelope<TSourceEntityId>
    {
        TParentId ParentId { get; }
    }
}