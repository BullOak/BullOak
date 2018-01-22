using BullOak.Repositories.Session;
using EventStore.ClientAPI;
using System;
using System.Linq;

namespace BullOak.Repositories.EventStore
{
    public class EventstoreRepository<TId, TState>
    {
        private readonly IEventStoreConnection connection;

        public EventstoreRepository(IEventStoreConnection connection)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public IManageAndSaveSessionWithSnapshot<TState> BeginSessionFor(TId id, bool throwIfNotExists = false)
        {
            connection.ConnectAsync().Wait();

            var stream = store.OpenStream(id.ToString(), 0);

            if (throwIfNotExists && stream.CommittedEvents.Count <= 0)
                throw new StreamNotFoundException();

            var session = new NEventStoreSession<TState>(configuration, stream);
            session.Initialize();
            return session;
        }

    }
}
