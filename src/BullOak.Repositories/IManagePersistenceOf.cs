namespace BullOak.Repositories
{
    using System.Threading.Tasks;
    using BullOak.Common;
    using BullOak.Repositories.Session;

    /// <summary>
    /// This interface represents the repository of given state using a specific type of session and id of state
    /// </summary>
    /// <typeparam name="TState">Type of state that the repository implementing this inteface will manage</typeparam>
    /// <typeparam name="TSession">The session that will be emitted</typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface IManagePersistenceOf<TState, in TId>
    {
        Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false);

        Task Clear(TId id);
        Task<bool> Exists(TId id);
    }
}