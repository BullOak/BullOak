namespace BullOak.Repositories
{
    using System;
    using BullOak.Repositories.Appliers;

    public interface IManuallyConfigureEventAppliers : IConfigureEventAppliers
    {
        IManuallyConfigureEventAppliers WithEventApplier<TState>(IApplyEvents<TState> stateApplier);
        IManuallyConfigureEventAppliers WithEventApplier<TState, TEvent>(IApplyEvent<TState, TEvent> stateApplier);
        IManuallyConfigureEventAppliers WithEventApplier(Type stateType, Type eventType, object applier);
        IManuallyConfigureEventAppliers WithEventApplier(Type stateType, object applier);

        //IManuallyConfigureEventAppliers WithEventApplier(Type typeOfState, object stateApplier);
        // IManuallyConfigureEventAppliers WithApplierFactory(Type typeOfState, Func<object> factory);
        IConfigureUpconverter AndNoMoreAppliers();
    }
    public interface IConfigureEventAppliers
    {
        IConfigureUpconverter WithEventApplier(IApplyEventsToStates eventApplier);
    }

    public interface IBuildConfiguration
    {
        IHoldAllConfiguration Build();
    }
}