namespace BullOak.Repositories.EventStore
{
    using BullOak.Repositories.Session;
    using global::EventStore.ClientAPI;
    using System;
    using System.Threading.Tasks;
    using BullOak.Repositories.Repository;

    public class EventStoreRepository<TId, TState> : IStartSessions<TId, TState>
    {
        private readonly IHoldAllConfiguration configs;
        private readonly IEventStoreConnection connection;

        public EventStoreRepository(IHoldAllConfiguration configs, IEventStoreConnection connection)
        {
            this.configs = configs ?? throw new ArgumentNullException(nameof(connection));
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            var session = new EventStoreSession<TState>(configs, connection, id.ToString());
            await session.Initialize();
            return session;
        }

        public Task<bool> Contains(TId selector)
        {
            throw new NotImplementedException();
        }

        public Task Delete(TId selector)
        {
            throw new NotImplementedException();
        }
    }
}
