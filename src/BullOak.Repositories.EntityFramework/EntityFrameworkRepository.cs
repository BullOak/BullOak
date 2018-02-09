namespace BullOak.Repositories.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using BullOak.Repositories.Repository;
    using BullOak.Repositories.Session;

    public class EntityFrameworkRepository<TContext, TState> : IStartSessions<Expression<Func<TState, bool>>, TState>
        where TState: class
        where TContext : DbContext
    {
        private static Type stateType = typeof(TState);

        protected readonly Func<TContext> dbContextFactory;
        protected readonly IHoldAllConfiguration configuration;

        public EntityFrameworkRepository(IHoldAllConfiguration configuration, Func<TContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IManageSessionOf<TState>> BeginSessionFor(Expression<Func<TState, bool>> entitySelector, bool throwIfNotExists = false)
        {
            TContext dbContext = null;
            EntityFrameworkSession<TState> session = null;
            try
            {
                dbContext = dbContextFactory();
                var set = dbContext.Set<TState>();

                var entity = await set.FirstOrDefaultAsync(entitySelector) ?? set.Create();
                var isExisting = set.Local.Contains(entity);

                session = new EntityFrameworkSession<TState>(configuration, dbContext, set, isExisting);
                //TODO (Savvas) -> wrap for editability before setting entity
                session.SetEntity(entity, !isExisting);

                return session;
            }
            finally
            {
                if (session == null) dbContext?.Dispose();
            }
        }

        public async Task Delete(Expression<Func<TState, bool>> selector)
        {
            using (var dbContext = dbContextFactory())
            {
                var set = dbContext.Set<TState>();
                var entity = await set.FirstOrDefaultAsync(selector);

                if (entity != null)
                {
                    set.Remove(entity);
                }

                await dbContext.SaveChangesAsync();
            }
        }

        public Task<bool> Contains(Expression<Func<TState, bool>> selector)
        {
            using (var dbContext = dbContextFactory())
            {
                var set = dbContext.Set<TState>();
                return set.AnyAsync(selector);
            }
        }
    }
}
