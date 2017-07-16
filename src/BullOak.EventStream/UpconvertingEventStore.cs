namespace BullOak.EventStream.Upconvert
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Messages.Converters;

    public class UpconvertingEventStore : IEventStore
    {
        private readonly IEventStore eventStore;
        private readonly RecursiveEventUpconverter recursiveEventUpconverter;

        public UpconvertingEventStore(IEventStore originalStore, params IEventConverter[] eventConverters)
            :this(originalStore, (IEnumerable<IEventConverter>)eventConverters)
        { }

        public UpconvertingEventStore(IEventStore originalStore, IEnumerable<IEventConverter> eventConverters)
        {
            if (originalStore == null) throw new ArgumentNullException(nameof(originalStore));

            this.eventStore = originalStore;
            this.recursiveEventUpconverter = new RecursiveEventUpconverter(eventConverters);
        }

        public Task<bool> Exists(string id)
        {
            return eventStore.Exists(id);
        }

        public async Task<EventStoreData> LoadFor(string id)
        {
            var originalData = await eventStore.LoadFor(id);

            return new EventStoreData(originalData.EventEnvelopes.SelectMany(
                x => recursiveEventUpconverter.UpconvertEvent(x.Event).Select(x.CloneWithEvent)),
                originalData.ConcurrencyId);
        }

        public async Task Store(string id, int concurrencyData, IEnumerable<IParcelVisionEventEnvelope> newEvents)
        {
            await eventStore.Store(id, concurrencyData, newEvents);
        }
    }
}
