namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventPublisher;
    using BullOak.Repositories.Middleware;
    using BullOak.Repositories.Rehydration;
    using BullOak.Repositories.StateEmit;
    using BullOak.Repositories.Upconverting;

    public interface IHoldAllConfiguration
    {
        Func<Type, Func<ICollection<ItemWithType>>> CollectionTypeSelector { get; }
        IPublishEvents EventPublisher { get; }
        IApplyEventsToStates EventApplier { get; }
        IRehydrateState StateRehydrator { get; }
        Func<Type, bool> ThreadSafetySelector { get; }
        ICreateStateInstances StateFactory { get; }
        IUpconvertStoredItems EventUpconverter { get; }
        bool HasInterceptors { get; }
        IInterceptEvents[] Interceptors { get; }
    }
}