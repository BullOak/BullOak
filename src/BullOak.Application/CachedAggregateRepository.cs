namespace BullOak.Application
{
    using System;
    using System.Threading.Tasks;
    using Common;
    using System.Runtime.Caching;

    // TODO (Richard) The in memory cache should be cleared between each test scenario
    public class CachedAggregateRepository<TAggregateRoot, TId> : IAggregateRepository<TAggregateRoot, TId>
        where TId : IId, IEquatable<TId>
        where TAggregateRoot : AggregateRoot<TId>, new()
    {
        private readonly IAggregateRepository<TAggregateRoot, TId> repository;

        public CachedAggregateRepository(IAggregateRepository<TAggregateRoot, TId> repository)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            this.repository = repository;
        }

        public async Task<bool> Exists(string id)
        {
            if (MemoryCache.Default.Contains(id)) return true;

            // the cache does not have the id so lets check the repository
            var exists = await repository.Exists(id);

            if (exists)
            {
                // store id in cache as it currently exists
                MemoryCache.Default.Add(id, bool.TrueString, null);
            }

            return exists;
        }

        public async Task<TAggregateRoot> Load(TId aggregateId, bool throwIfNotFound = true)
        {
            return await repository.Load(aggregateId, throwIfNotFound);
        }

        public async Task Save(TAggregateRoot aggregateRoot)
        {
            await repository.Save(aggregateRoot);
        }
    }
}