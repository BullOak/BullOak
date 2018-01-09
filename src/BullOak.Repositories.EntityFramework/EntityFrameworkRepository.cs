namespace BullOak.Repositories.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using BullOak.Repositories.Session;

    public class EntityFrameworkRepository<TContext>
        where TContext : DbContext
    {
        protected readonly Func<TContext> dbContextFactory;
        protected readonly IHoldAllConfiguration configuration;

        public EntityFrameworkRepository(IHoldAllConfiguration configuration, Func<TContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IManageAndSaveSession<TState> BeginSessionWithNewEntity<TState, TEntityFrameworkEntity>(
            TEntityFrameworkEntity newEntity)
            where TEntityFrameworkEntity : class, TState
        {
            TContext dbContext = null;
            EntityFrameworkSession<TState> session = null;
            try
            {
                dbContext = dbContextFactory();
                session = new EntityFrameworkSession<TState>(configuration, dbContext);
                dbContext.Set<TEntityFrameworkEntity>()
                    .Add(newEntity);
                session.SetEntity(newEntity);
                return session;
            }
            finally
            {
                if (session == null) dbContext?.Dispose();
            }
        }

        public IManageAndSaveSession<TState> BeginSessionFor<TState>(Func<TContext, TState> entitySelector)
        {
            TContext dbContext = null;
            EntityFrameworkSession<TState> session = null;
            try
            {
                dbContext = dbContextFactory();
                session = new EntityFrameworkSession<TState>(configuration, dbContext);
                session.SetEntity(entitySelector(dbContext));
                return session;
            }
            finally
            {
                if (session == null) dbContext?.Dispose();
            }
        }

        public async Task<IManageAndSaveSession<TState>> BeginSessionFor<TState>(
            Func<TContext, Task<TState>> entitySelector)
        {
            TContext dbContext = null;
            EntityFrameworkSession<TState> session = null;
            try
            {
                dbContext = dbContextFactory();
                session = new EntityFrameworkSession<TState>(configuration, dbContext);
                var state = await entitySelector(dbContext);
                session.SetEntity(state);

                return session;
            }
            finally
            {
                if (session == null) dbContext?.Dispose();
            }
        }
    }
}
