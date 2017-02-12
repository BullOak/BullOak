namespace BullOak.Application.Test.Unit.Aggregate
{
    using System;
    using System.Runtime.Serialization;
    using BullOak.Common;

    [DataContract]
    public struct SubEntityId : IId, IEquatable<SubEntityId>
    {
        [DataMember]
        public Guid Id { get; private set; }

        public static implicit operator Guid(SubEntityId id)
        {
            return id.Id;
        }

        public static explicit operator SubEntityId(Guid id)
        {
            var entityId = new SubEntityId { Id = id };

            return entityId;
        }

        public bool Equals(SubEntityId other)
        {
            return Id == other.Id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}