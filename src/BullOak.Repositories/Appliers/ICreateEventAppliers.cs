namespace BullOak.Repositories
{
    using System.Collections.Generic;
    using BullOak.Repositories.Appliers;

    public interface ICreateEventAppliers
    {
        IEnumerable<IApplyEvents<TState>> GetInstance<TState>();
    }
}