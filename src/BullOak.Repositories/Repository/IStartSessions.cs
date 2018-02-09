namespace BullOak.Repositories.Repository
{
    using System.Threading.Tasks;
    using BullOak.Repositories.Session;

    public interface IStartSessions<in TEntitySelector, TState>
    {
        Task<IManageSessionOf<TState>> BeginSessionFor(TEntitySelector selector, bool throwIfNotExists);
        Task Delete(TEntitySelector selector);
        Task<bool> Contains(TEntitySelector selector);
    }
}