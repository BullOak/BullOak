namespace BullOak.Repositories.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Linq;
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
                session.SetEntity(newEntity, true);
                return session;
            }
            finally
            {
                if (session == null) dbContext?.Dispose();
            }
        }

        public IManageAndSaveSession<TState> BeginSessionFor<TState>(Func<TContext, TState> entitySelector)
            where TState : class
        {
            TContext dbContext = null;
            EntityFrameworkSession<TState> session = null;
            try
            {
                dbContext = dbContextFactory();
                session = new EntityFrameworkSession<TState>(configuration, dbContext);
                var state = entitySelector(dbContext);

                var isExisting = AttachAndReturnIfAlreadyAttached(state, dbContext);

                session.SetEntity(state, !isExisting);
                return session;
            }
            finally
            {
                if (session == null) dbContext?.Dispose();
            }
        }

        public async Task<IManageAndSaveSession<TState>> BeginSessionFor<TState>(
            Func<TContext, Task<TState>> entitySelector)
            where TState: class
        {
            TContext dbContext = null;
            EntityFrameworkSession<TState> session = null;
            try
            {
                dbContext = dbContextFactory();
                session = new EntityFrameworkSession<TState>(configuration, dbContext);
                var state = await entitySelector(dbContext);

                var isExisting = AttachAndReturnIfAlreadyAttached(state, dbContext);

                session.SetEntity(state, !isExisting);

                return session;
            }
            finally
            {
                if (session == null) dbContext?.Dispose();
            }
        }

        private static bool AttachAndReturnIfAlreadyAttached(object state, TContext dbContext)
        {
            var set = dbContext.Set(state.GetType());
            var isAttached = set.Local.Contains(state);

            if(!isAttached) set.Attach(state);

            return isAttached;
        }
    }
}
