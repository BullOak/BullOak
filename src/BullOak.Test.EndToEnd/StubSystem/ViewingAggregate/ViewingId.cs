namespace BullOak.Test.EndToEnd.StubSystem.ViewingAggregate
{
    using BullOak.Common;
    using BullOak.Test.EndToEnd.StubSystem.CinemaAggregate;
    using System;

    internal class ViewingId : IId, IEquatable<ViewingId>
    {
        public string MovieName { get; private set; }
        public DateTime ShowingDate { get; private set; }
        public CinemaAggregateRootId CinemaId { get; private set; }

        public ViewingId(string movieName, DateTime showingDate, CinemaAggregateRootId cinemaId)
        {
            MovieName = movieName;
            ShowingDate = showingDate;
            CinemaId = cinemaId;
        }

        public override string ToString() => $"{MovieName}-{ShowingDate.ToString("yyMMdd")}-{CinemaId}";
        public override int GetHashCode() => MovieName.GetHashCode() ^ ShowingDate.GetHashCode() ^ CinemaId.GetHashCode();
        public bool Equals(ViewingId other) => MovieName == other?.MovieName
                && ShowingDate == other?.ShowingDate
                && CinemaId == other?.CinemaId;
        public override bool Equals(object obj) => Equals(obj as ViewingId);
    }
}
