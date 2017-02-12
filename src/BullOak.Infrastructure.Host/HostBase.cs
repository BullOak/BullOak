namespace BullOak.Infrastructure.Host
{
    using System;

    public abstract class HostBase : MarshalByRefObject, IHost
    {
        public abstract IDisposable Start();
    }
}
