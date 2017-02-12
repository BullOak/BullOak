namespace BullOak.Infrastructure.TestHelpers.Application.Stubs
{
    using System;
    using BullOak.Application;

    public class FixedClock : IClock
    {
        private readonly DateTime date;

        public FixedClock(DateTime date)
        {
            if (date.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("This clock only works int UTC time.");
            }

            this.date = date;
        }

        public DateTime UtcNow => date;
    }
}
