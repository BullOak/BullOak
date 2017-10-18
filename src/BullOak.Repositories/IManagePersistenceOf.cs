namespace BullOak.Repositories
{
    using System.Threading.Tasks;
    using BullOak.Common;

    public interface IManagePersistenceOf<TState, out TSessionManager, in TId>
        where TSessionManager : IManageSessionOf<TState>
        where TId : IId
    {
        TSessionManager Load(TId id, bool throwIfNotExists = false);

        Task Clear(TId id);
        Task<bool> Exists(TId id);
    }
}