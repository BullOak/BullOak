namespace BullOak.Repositories.EventStore
{
    using global::EventStore.ClientAPI;

    public static class EventstoreConnectionExtensions
    {
        public static void SafeClose(this IEventStoreConnection connection)
        {
            try
            {
                if (connection == null)
                {
                    return;
                }

                connection.Close();
                connection.Dispose();

            }
            catch
            {
                // ignored
            }
        }
    }
}