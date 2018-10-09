namespace BullOak.Repositories.EventStore
{
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Repository;
    using BullOak.Repositories.Session;
    using global::EventStore.ClientAPI;
    using System;
    using System.Threading.Tasks;

    public class EventStoreRepository<TId, TState> : IStartSessions<TId, TState>
    {
        private readonly IHoldAllConfiguration configs;
        private readonly Func<IEventStoreConnection> connectionFactory;

        public EventStoreRepository(IHoldAllConfiguration configs, Func<IEventStoreConnection> connectionFactory)
        {
            this.configs = configs ?? throw new ArgumentNullException(nameof(configs));
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            IEventStoreConnection connection;
            try
            {
                connection = connectionFactory();
            }
            catch (Exception ex)
            {
                throw new RepositoryUnavailableException("Couldn't connect to the EvenStore repository. See InnerException for details", ex);
            }

            if (connection == null)
            {
                throw new RepositoryUnavailableException("Couldn't connect to the EvenStore repository. Connection object is null.");
            }

            using (connection)
            {
                if (throwIfNotExists && !(await Contains(id, connection)))
                {
                    throw new StreamNotFoundException(id.ToString());
                }

                var session = new EventStoreSession<TState>(configs, connectionFactory(), id.ToString());
                await session.Initialize();

                return session;
            }
        }

        private async Task<bool> Contains(TId selector, IEventStoreConnection connection)
        {
            try
            {
                var id = selector.ToString();
                var eventsTail = await connection.ReadStreamEventsForwardAsync(id, 0, 1, false);
                return eventsTail.Status == SliceReadStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<bool> Contains(TId selector)
        {
            using (var connection = connectionFactory())
            {
                return Contains(selector, connection);
            }
        }


        public async Task Delete(TId selector)
        {
            using (var connection = connectionFactory())
            {
                var id = selector.ToString();
                var eventsTail = await connection.ReadStreamEventsBackwardAsync(id, 0, 1, false);
                var expectedVersion = eventsTail.LastEventNumber;
                await connection.DeleteStreamAsync(id, expectedVersion);
            }
        }
    }
}
