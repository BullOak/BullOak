namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BullOak.Common;
    using BullOak.Repositories.EventSourced;
    using global::NEventStore;

    internal class NEventStoreRepository<TState, TId> : IManagePersistenceOf<TState, IManageEventSourceSession<TState>, TId>
        where TState : new()
        where TId : IId
    {
        private readonly IStoreEvents store;
        private readonly ICreateEventAppliers appliersFactory;

        public NEventStoreRepository(IStoreEvents store, ICreateEventAppliers appliersFactory)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.appliersFactory = appliersFactory ?? throw new ArgumentNullException(nameof(appliersFactory));
        }

        public Task Clear(TId id)
        {
            store.Advanced.DeleteStream("bucketId", id.ToString());
            return Task.FromResult(true);
        }

        public Task<bool> Exists(TId id)
        {
            try
            {
                store.OpenStream("bucketId", id.ToString(), int.MinValue, int.MaxValue);
                return Task.FromResult(true);
            }
            catch (StreamNotFoundException)
            {
                return Task.FromResult(false);
            }
        }

        public IManageEventSourceSession<TState> Load(TId id, bool throwIfNotExists = true)
        {
            var stream = store.OpenStream(streamId: id.ToString());

            var events = stream.CommittedEvents
                .Select(x => x.Body)
                .Cast<IHoldEventWithMetadata>()
                .ToArray();

            var session = new NEventStoreSession<TState>(appliersFactory, stream);
            session.Initialize(events, stream.StreamRevision);
            return session;
        }
    }
}
