namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Threading.Tasks;
    using BullOak.Repositories.Repository;
    using BullOak.Repositories.Session;
    using global::NEventStore;

    public class NEventStoreRepository<TId, TState> : IStartSessions<TId, TState>
    {
        private static readonly Task<bool> falseResult = Task.FromResult(false);
        private static readonly Task<bool> trueResult = Task.FromResult(true);

        private readonly IStoreEvents store;
        private readonly IHoldAllConfiguration configuration;

        public NEventStoreRepository(IStoreEvents store, IHoldAllConfiguration configuration)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            var stream = store.OpenStream(id.ToString(), 0);

            if (throwIfNotExists && stream.CommittedEvents.Count <=0)
                throw new StreamNotFoundException();

            var session = new NEventStoreSession<TState>(configuration, stream);
            session.Initialize();
            return Task.FromResult((IManageSessionOf<TState>)session);
        }

        public Task<bool> Contains(TId id)
        {
            try
            {
                var stream = store.OpenStream("bucketId", id.ToString(), int.MinValue, int.MaxValue);
                return stream.CommittedEvents.Count > 0 ? trueResult : falseResult;
            }
            catch (StreamNotFoundException)
            {
                return falseResult;
            }
        }

        public Task Delete(TId id)
        {
            store.Advanced.DeleteStream("bucketId", id.ToString());
            return falseResult;
        }
    }
}
