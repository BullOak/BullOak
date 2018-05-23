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
        private static readonly Task<bool> falseResult = Task.FromResult(false);
        private readonly IHoldAllConfiguration configs;
        private readonly IEventStoreConnection connection;

        public EventStoreRepository(IHoldAllConfiguration configs, IEventStoreConnection connection)
        {
            this.configs = configs ?? throw new ArgumentNullException(nameof(connection));
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<IManageSessionOf<TState>> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            if (throwIfNotExists && !(await Contains(id)))
                throw new StreamNotFoundException(id.ToString());

            var session = new EventStoreSession<TState>(configs, connection, id.ToString());
            await session.Initialize();

            return session;
        }

        public async Task<bool> Contains(TId selector)
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

        public async Task Delete(TId selector)
        {
            var id = selector.ToString();
            var eventsTail = await connection.ReadStreamEventsBackwardAsync(id, 0, 1, false);
            var expectedVersion = eventsTail.LastEventNumber;
            await connection.DeleteStreamAsync(id, expectedVersion);
        }
    }
}
