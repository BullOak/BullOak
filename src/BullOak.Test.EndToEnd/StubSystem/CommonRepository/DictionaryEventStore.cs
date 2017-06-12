namespace BullOak.Test.EndToEnd.StubSystem.CommonRepository
{
    using BullOak.EventStream;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class DictionaryEventStore : IEventStore
    {
        public Dictionary<string, List<ParcelVisionEventEnvelope>> DictionaryStorage { get; set; }

        public DictionaryEventStore()
        {
            DictionaryStorage = new Dictionary<string, List<ParcelVisionEventEnvelope>>();
        }

        public Task<bool> Exists(string id)
        {
            return Task.FromResult(DictionaryStorage.ContainsKey(id));
        }

        public Task<EventStoreData> LoadFor(string id)
        {
            throw new NotImplementedException();
        }

        public Task Store(string id, int concurrencyId, IEnumerable<IParcelVisionEventEnvelope> newEvents)
        {
            throw new NotImplementedException();
        }
    }
}
