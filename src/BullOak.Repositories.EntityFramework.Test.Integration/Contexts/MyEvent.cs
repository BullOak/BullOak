namespace BullOak.Repositories.EntityFramework.Test.Integration.Contexts
{
    using System;

    public class MyEvent
    {
        public Guid ClientId { get; private set; }
        public int Order { get; private set; }

        public MyEvent(int order)
        {
            ClientId = Guid.NewGuid();
            Order = order;
        }
    }

    public class InitializeClientOrderEvent
    {
        public Guid ClientId { get; private set; }

        public InitializeClientOrderEvent(Guid clientId)
            => ClientId = clientId;
    }
}