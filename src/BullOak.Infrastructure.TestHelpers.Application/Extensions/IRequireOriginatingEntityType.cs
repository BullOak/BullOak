namespace BullOak.Infrastructure.TestHelpers.Application.Extensions
{
    using System;
    using BullOak.Application;
    using BullOak.Common;
    using BullOak.Messages;

    public interface IRequireOriginatingEntityType<TEvent, TSourceId>
        where TEvent : IParcelVisionEvent
        where TSourceId : IId, IEquatable<TSourceId>
    {
        ParcelVisionEventEnvelope FromAggregate<TSelf>() where TSelf : AggregateRoot<TSourceId>;
        IRequireParentIdType<TEvent, TSourceId> FromChildEntity<TEntity>() where TEntity : Entity<TSourceId>;
    }
}