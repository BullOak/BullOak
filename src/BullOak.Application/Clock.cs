namespace BullOak.Application
{
    using System;

    public class Clock : IClock
    {
        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
