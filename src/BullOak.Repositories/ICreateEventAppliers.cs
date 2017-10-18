namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;

    public interface ICreateEventAppliers
    {
        IEnumerable<IApplyEvents<TState>> GetInstance<TState>();
    }
}
