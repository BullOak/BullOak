namespace BullOak.Repositories.Session
{
    using System;
    using System.Threading.Tasks;

    public class SimpleSessionWithAmbientConcurrency<TState> : RepoSessionWithoutConcurrency<TState>
    {
        private readonly Func<object[], TState, Task> saveFunc;

        public SimpleSessionWithAmbientConcurrency(IHoldAllConfiguration configuration,
            Func<object[], TState, Task> saveFunc)
            : base(configuration)
        {
            this.saveFunc = saveFunc ?? throw new ArgumentNullException(nameof(saveFunc));
        }

        protected sealed override Task SaveChangesProtected(object[] newEvents,
            TState latestState, bool eventsAlreadyPublished)
            => saveFunc(newEvents, latestState);
    }

    public class SimpleSessionWithConcurrencyId<TState, TConcurrencyId> : RepoSessionWithConcurrency<TState, TConcurrencyId>
    {
        private readonly Func<object[], TState, TConcurrencyId, Task> saveFunc;

        public SimpleSessionWithConcurrencyId(IHoldAllConfiguration configuration,
            Func<object[], TState, TConcurrencyId, Task> saveFunc)
            : base(configuration)
        {
            this.saveFunc = saveFunc ?? throw new ArgumentNullException(nameof(saveFunc));
        }

        protected sealed override Task SaveChangesProtected(object[] newEvents,
            TState latestState, TConcurrencyId concurrencyId, bool eventsAlreadyPublished)
            => saveFunc(newEvents, latestState, concurrencyId);
    }
}
