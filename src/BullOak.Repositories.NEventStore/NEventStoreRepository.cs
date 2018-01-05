namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Common;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Session;
    using CommonDomain;
    using global::NEventStore;

    internal class NEventStoreRepository<TId, TState>: ISynchronouslyManagePersistanceOf<TId, NEventStoreSession<TState>, TState>
    {
        private readonly IStoreEvents store;
        private readonly IHoldAllConfiguration configuration;

        public NEventStoreRepository(IStoreEvents store, IHoldAllConfiguration configuration)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public NEventStoreSession<TState> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            var stream = store.OpenStream(streamId: id.ToString());
            var sn = new Snapshot("", 0, null);

            store.OpenStream(sn, int.MaxValue);
            var events = stream.CommittedEvents
                .Select(x => x.Body)
                .ToArray();

            var session = new NEventStoreSession<TState>(configuration, stream);

            return session;
        }

        public void Clear(TId id) 
            => store.Advanced.DeleteStream("bucketId", id.ToString());

        public bool Exists(TId id)
        {
            try
            {
                var stream = store.OpenStream("bucketId", id.ToString(), int.MinValue, int.MaxValue);
                return stream.CommittedEvents.Count > 0;
            }
            catch (StreamNotFoundException)
            {
                return false;
            }
        }
    }
}
