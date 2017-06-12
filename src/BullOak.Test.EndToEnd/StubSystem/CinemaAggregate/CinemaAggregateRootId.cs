namespace BullOak.Test.EndToEnd.StubSystem.CinemaAggregate
{
    using BullOak.Common;
    using System;

    internal class CinemaAggregateRootId : IId, IEquatable<CinemaAggregateRootId>
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
