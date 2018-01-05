namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Linq;
    using BullOak.Repositories.Session;
    using global::NEventStore;

    public class NEventStoreRepository<TId, TState>: ISynchronouslyManagePersistanceOf<TId,
        IManageAndSaveSynchronousSession<TState>, TState>
    {
        private readonly IStoreEvents store;
        private readonly IHoldAllConfiguration configuration;

        public NEventStoreRepository(IStoreEvents store, IHoldAllConfiguration configuration)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IManageAndSaveSynchronousSession<TState> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            var stream = store.OpenStream(id.ToString(), 0);

            if (throwIfNotExists && stream.CommittedEvents.Count <=0)
                throw new StreamNotFoundException();

            var session = new NEventStoreSession<TState>(configuration, stream);
            session.Initialize();
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
