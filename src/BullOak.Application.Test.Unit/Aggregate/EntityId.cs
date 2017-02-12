namespace BullOak.Application.Test.Unit.Aggregate
{
    using System;
    using System.Runtime.Serialization;
    using BullOak.Common;

    [DataContract]
    public struct EntityId : IId, IEquatable<EntityId>
    {
        [DataMember]
        public Guid Id { get; private set; }

        public static implicit operator Guid(EntityId id)
        {
            return id.Id;
        }

        public static explicit operator EntityId(Guid id)
        {
            var entityId = new EntityId { Id = id };

            return entityId;
        }

        public bool Equals(EntityId other)
        {
            return Id == other.Id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}