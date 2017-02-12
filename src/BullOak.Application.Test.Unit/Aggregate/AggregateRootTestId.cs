namespace BullOak.Application.Test.Unit.Aggregate
{
    using System;
    using System.Runtime.Serialization;
    using BullOak.Common;

    [DataContract]
    public struct AggregateRootTestId : IId, IEquatable<AggregateRootTestId>
    {
        [DataMember]
        public Guid Id { get; private set; }

        public static implicit operator Guid(AggregateRootTestId id)
        {
            return id.Id;
        }

        public static explicit operator AggregateRootTestId(Guid id)
        {
            var sutId = new AggregateRootTestId { Id = id };

            return sutId;
        }

        public bool Equals(AggregateRootTestId other)
        {
            return Id == other.Id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}