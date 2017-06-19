using System.Collections.Concurrent;

namespace BullOak.Infrastructure.TestHelpers.Application.Stubs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.EventStream;
    using BullOak.Messages;

    public class InMemoryEventStore : IEventStore
    {
        private ConcurrentDictionary<string, List<IParcelVisionEventEnvelope>> memoryStore = 
            new ConcurrentDictionary<string, List<IParcelVisionEventEnvelope>>();

        public List<IParcelVisionEventEnvelope> this[string id] => GetOrCreateEntryFor(id);

        public Task<bool> Exists(string id)
        {
            List<IParcelVisionEventEnvelope> entry;

            return Task.FromResult(memoryStore.TryGetValue(id, out entry));
        }

        public void Clear() => memoryStore.Clear();

        public Task<EventStoreData> LoadFor(string id)
        {
            var entry = GetOrCreateEntryFor(id);

            return Task.FromResult(new EventStoreData(entry, entry.Count));
        }

        private List<IParcelVisionEventEnvelope> GetOrCreateEntryFor(string id)
            => memoryStore.GetOrAdd(id, _ => new List<IParcelVisionEventEnvelope>());

        public Task Store(string id, int concurrencyId, IEnumerable<IParcelVisionEventEnvelope> newEvents)
        {
            List<IParcelVisionEventEnvelope> eventList;

            if (memoryStore.TryGetValue(id, out eventList))
            {
                if (concurrencyId != eventList.Count) throw new ConcurrencyException(id);
            }
            else
            {
                eventList = new List<IParcelVisionEventEnvelope>();
            }

            eventList.AddRange(newEvents);

            memoryStore[id] = eventList;

            return Task.Delay(0);
        }
    }
}