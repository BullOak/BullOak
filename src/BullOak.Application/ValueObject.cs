namespace BullOak.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ValueObject<TValueObject, TAttributeValue> : IEquatable<TValueObject>, IEquatable<ValueObject<TValueObject, TAttributeValue>>
        where TValueObject : ValueObject<TValueObject, TAttributeValue>
    {
        protected abstract IEnumerable<TAttributeValue> GetAttributesToIncludeInEqualityCheck();

        public override bool Equals(object other) => Equals(other as TValueObject);

        public bool Equals(TValueObject other)
            =>
            other != null &&
            GetAttributesToIncludeInEqualityCheck().SequenceEqual(other.GetAttributesToIncludeInEqualityCheck());

        public bool Equals(ValueObject<TValueObject, TAttributeValue> other)
            =>
            other != null &&
            GetAttributesToIncludeInEqualityCheck().SequenceEqual(other.GetAttributesToIncludeInEqualityCheck());

        public static bool operator ==(ValueObject<TValueObject, TAttributeValue> left, ValueObject<TValueObject, TAttributeValue> right) 
            => left?.Equals(right) == true;

        public static bool operator !=(ValueObject<TValueObject, TAttributeValue> left, ValueObject<TValueObject, TAttributeValue> right)
            => left?.Equals(right) == false;

        public override int GetHashCode()
        {
            var hash = 17;
            foreach (var obj in this.GetAttributesToIncludeInEqualityCheck())
                hash = hash * 31 + obj?.GetHashCode() ?? 0;

            return hash;
        }
    }
}