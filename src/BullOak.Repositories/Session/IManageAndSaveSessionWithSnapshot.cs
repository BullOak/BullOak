namespace BullOak.Repositories.Session
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IManageAndSaveSessionWithSnapshot<out TState> : IManageAndSaveSession<TState>
    {
        int SaveChangesWithSnapshotSync(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce);

        Task<int> SaveChangesWithSnapshot(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce,
            CancellationToken? cancellationToken = null);
    }
}