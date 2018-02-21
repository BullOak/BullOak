namespace BullOak.Repositories.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Exceptions;
    using BullOak.Repositories.Session;

    public class EntityFrameworkSession<TState> : BaseRepoSession<TState>
        where TState : class
    {
        private readonly DbContext dbContext;

        private static readonly Type stateType;
        private static readonly bool canWrap;
        public static bool CanWrap => canWrap;

        private readonly bool useStateImmutabilityWrapping;
        private readonly DbSet<TState> set;
        private readonly bool isNew;

        static EntityFrameworkSession()
        {
            stateType = typeof(TState);
            canWrap = stateType.IsInterface;
        }

        public EntityFrameworkSession(IHoldAllConfiguration configuration,
            DbContext dbContext,
            DbSet<TState> set,
            bool isNew,
            bool useStateImmutabilityWrapping = true)
            : base(configuration, dbContext)
        {
            this.isNew = isNew;
            this.set = set;
            this.dbContext = dbContext;
            this.useStateImmutabilityWrapping = useStateImmutabilityWrapping;
        }

        internal void SetEntity(TState state, bool isNewEntity)
        {
            if (canWrap && useStateImmutabilityWrapping)
            {
                var wrapperFactory = configuration.StateFactory.GetWrapper<TState>();
                state = wrapperFactory(state);
            }

            this.Initialize(state, isNewEntity);
        }

        protected override async Task<int> SaveChanges(ItemWithType[] newEvents,
            TState currentState,
            CancellationToken? cancellationToken)
        {
            if (isNew) set.Attach(currentState);

            try
            {
                return await (cancellationToken != null
                    ? dbContext.SaveChangesAsync(cancellationToken.Value)
                    : dbContext.SaveChangesAsync());
            }
            catch (OptimisticConcurrencyException oce)
            {
                throw new ConcurrencyException(typeof(TState), oce);
            }
        }
    }
}