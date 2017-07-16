namespace BullOak.Repositories
{
    using System.Threading.Tasks;
    using BullOak.Common;

    public interface IPersistEntities
    {
        Task<IManageStoreRequestLifetime<TState>> Load<TId, TState>(TId id, bool throwIfNotExists = true)
            where TId : IId;
        Task Clear<TId>(TId id)
            where TId : IId;
        Task<bool> Exists<TId>(TId id)
            where TId : IId;
    }
}