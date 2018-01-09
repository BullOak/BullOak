namespace BullOak.Repositories.Session
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IManageAndSaveSession<out TState> : IManageSessionOf<TState>
    {
        int SaveChangesSync(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce);

        Task<int> SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce,
            CancellationToken? cancellationToken = null);
    }
}