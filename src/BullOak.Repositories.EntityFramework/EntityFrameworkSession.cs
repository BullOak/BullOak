namespace BullOak.Repositories.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.Session;

    public class EntityFrameworkSession<TState> : BaseRepoSession<TState>, IManageAndSaveSession<TState>
    {
        private readonly DbContext dbContext;
        public override bool IsOptimisticConcurrencySupported => false;

        private static readonly Type stateType;
        private static readonly bool canWrap;
        public static bool CanWrap => canWrap;

        private readonly bool useStateImmutabilityWrapping;

        static EntityFrameworkSession()
        {
            stateType = typeof(TState);
            canWrap = stateType.IsInterface;
        }

        public EntityFrameworkSession(IHoldAllConfiguration configuration,
            DbContext dbContext,
            bool useStateImmutabilityWrapping = true)
            : base(configuration, dbContext)
        {
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

        public int SaveChangesSync(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce)
        {
            if (targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce)
            {
                PublishEventsSync(configuration, this.NewEventsCollection.ToArray());
            }

            var result = dbContext.SaveChanges();

            if (targetGuarantee == DeliveryTargetGuarntee.AtMostOnce)
            {
                PublishEventsSync(configuration, this.NewEventsCollection.ToArray());
            }

            return result;
        }

        public async Task<int> SaveChanges(DeliveryTargetGuarntee targetGuarantee = DeliveryTargetGuarntee.AtLeastOnce,
            CancellationToken? cancellationToken = null)
        {
            if (targetGuarantee == DeliveryTargetGuarntee.AtLeastOnce)
            {
                await PublishEvents(configuration, this.NewEventsCollection.ToArray(), cancellationToken);
            }

            var result = await (cancellationToken != null
                ? dbContext.SaveChangesAsync(cancellationToken.Value)
                : dbContext.SaveChangesAsync());

            if (targetGuarantee == DeliveryTargetGuarntee.AtMostOnce)
            {
                await PublishEvents(configuration, this.NewEventsCollection.ToArray(), cancellationToken);
            }

            return result;
        }
    }
}
