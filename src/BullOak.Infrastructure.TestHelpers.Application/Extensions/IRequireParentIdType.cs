namespace BullOak.Infrastructure.TestHelpers.Application.Extensions
{
    using BullOak.Common;
    using BullOak.Messages;

    public interface IRequireParentIdType<TEvent, TSourceId>
        where TEvent : IParcelVisionEvent
        where TSourceId : IId
    {
        ParcelVisionEventEnvelope WithParentId<TParentId>(TParentId parentId);
    }
}