namespace BullOak.EventStream
{
    using BullOak.Messages;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        Task<bool> Exists(string id);

        Task<EventStoreData> LoadFor(string id);

        Task Store(string id, int concurrencyId, IEnumerable<IParcelVisionEventEnvelope> newEvents);       
    }
}