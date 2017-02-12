namespace BullOak.EventStream
{
    using System.Threading.Tasks;

    public interface IEventCleaner
    {
        Task ClearEventStore();
    }
}
