namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;

    public class TimeOfLastBalanceUpdateSetEvent
    {
        public DateTime LastUpdateTimeStamp { get; set; }

        public TimeOfLastBalanceUpdateSetEvent(DateTime lastUpdated)
            => LastUpdateTimeStamp = lastUpdated;
    }
}
