namespace BullOak.Infrastructure.Host
{
    using System;

    public interface IHost
    {
        IDisposable Start();
    }
}
