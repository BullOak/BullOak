namespace BullOak.Test.EndToEnd.Stub.Shared.Ids
{
    using System;
    public class CinemaAggregateRootId : IEquatable<CinemaAggregateRootId>
    {
        public string Name { get; }

        public CinemaAggregateRootId(string name)
        {
            Name = name;
        }

        public bool Equals(CinemaAggregateRootId other) => Name == other?.Name;
        public override bool Equals(object other) => Equals(other as CinemaAggregateRootId);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name;
    }
}
