namespace BullOak.Repositories.Appliers
{
    using System;

    internal class EventAndStateTypes : IEquatable<EventAndStateTypes>
    {
        public Type stateType;
        public Type eventType;
        private int hashCode;

        public EventAndStateTypes(Type stateType, Type eventType)
        {
            this.stateType = stateType;
            this.eventType = eventType;

            unchecked
            {
                hashCode = (stateType.GetHashCode() * 397) ^ eventType.GetHashCode();
            }
        }

        /// <inheritdoc />
        public bool Equals(EventAndStateTypes other) 
            => ReferenceEquals(stateType, other.stateType) && ReferenceEquals(eventType, other.eventType);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EventAndStateTypes) obj);
        }

        public override int GetHashCode() => hashCode;
    }
}
