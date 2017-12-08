namespace BullOak.Repositories
{
    using BullOak.Repositories.Appliers;

    public interface IManuallyConfigureEventAppliers : IConfigureEventAppliers
    {
        IManuallyConfigureEventAppliers WithEventApplier<TState>(IApplyEvents<TState> stateApplier);
        IBuildConfiguration AndNoMoreAppliers();
    }
    public interface IConfigureEventAppliers : IBuildConfiguration
    {
        IBuildConfiguration WithEventApplier(IApplyEventsToStates eventApplier);
    }

    public interface IBuildConfiguration
    {
        IHoldAllConfiguration Build();
    }
}