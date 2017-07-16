namespace BullOak.Repositories.NEventStore
{
    using System;
    using System.Threading.Tasks;
    using BullOak.Common;
    using global::NEventStore;

    internal class NEventStoreRepository : IPersistEntities
    {
        private readonly IStoreEvents store;

        public NEventStoreRepository(IStoreEvents store) => this.store = store;

        public Task Clear<TId>(TId id)
            where TId : IId
        {
            store.Advanced.DeleteStream("bucketId", id.ToString());
            return Task.FromResult(true);
        }

        public Task<bool> Exists<TId>(TId id)
            where TId : IId
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

        public Task<IManageStoreRequestLifetime<TState>> Load<TId, TState>(TId id, bool throwIfNotExists = true)
            where TId : IId
        {
            throw new NotImplementedException();
        }
    }
}
