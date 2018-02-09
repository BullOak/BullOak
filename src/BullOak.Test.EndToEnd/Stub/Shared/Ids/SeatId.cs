namespace BullOak.Test.EndToEnd.Stub.Shared.Ids
{
    using System;

    public class SeatId : IEquatable<SeatId>
    {
        public ushort Id { get; private set; }

        public SeatId(ushort id)
        {
            Id = id;
        }

        public override string ToString() => Id.ToString();
        public override int GetHashCode() => Id.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as SeatId);
        public bool Equals(SeatId other) => Id == other?.Id;
    }
}
