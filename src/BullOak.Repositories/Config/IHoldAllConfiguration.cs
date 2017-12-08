namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.StateEmit;

    public interface IHoldAllConfiguration
    {
        Func<Type, Func<ICollection<object>>> CollectionTypeSelector { get; }
        Func<object, Task> EventPublisher { get; }
        IApplyEventsToStates EventApplier { get; }
        Func<Type, bool> ThreadSafetySelector { get; }
        ICreateStateInstances StateFactory { get; }
    }
}