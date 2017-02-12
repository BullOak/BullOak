namespace BullOak.Application
{
    using System;

    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
