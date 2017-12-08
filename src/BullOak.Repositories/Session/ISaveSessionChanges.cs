using System.Threading.Tasks;

namespace BullOak.Repositories.Session
{
    public interface ISaveSessionChanges
    {
        Task SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce);
    }
}