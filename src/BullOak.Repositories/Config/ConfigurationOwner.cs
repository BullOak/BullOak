using System;
using System.Collections.Generic;

//The above are outside so that the usings inside to be easier to read\write

namespace BullOak.Repositories
{
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.EventPublisher;
    using BullOak.Repositories.StateEmit;
    using StateTypeToCollectionTypeSelector = Func<Type, Func<ICollection<object>>>;
    using ThreadSafetySelector = Func<Type, bool>;

    public class ConfigurationOwner : IConfigureEventCollectionType, IConfigureStateFactory, IConfigureThreadSafety, IConfigureEventPublisher,
        IHoldAllConfiguration, IBuildConfiguration, IConfigureEventAppliers
    {
        public StateTypeToCollectionTypeSelector CollectionTypeSelector { get; private set; }
        public IPublishEvents EventPublisher { get; private set; }
        public IApplyEventsToStates EventApplier { get; private set; }
        public ThreadSafetySelector ThreadSafetySelector { get; private set; }
        public ICreateStateInstances StateFactory { get; private set; }

        public ConfigurationOwner() { }

        public IConfigureEventCollectionType Start() => this;

        IConfigureStateFactory IConfigureEventCollectionType.WithEventCollectionSelector(
            StateTypeToCollectionTypeSelector selectorOfCollectionTypeFromStateType)
        {
            CollectionTypeSelector = selectorOfCollectionTypeFromStateType;
            return this;
        }

        IConfigureThreadSafety IConfigureStateFactory.WithDefaultStateFactory()
            => (this as IConfigureStateFactory).WithStateFactory(new EmittedTypeFactory());

        IConfigureThreadSafety IConfigureStateFactory.WithStateFactory(ICreateStateInstances stateFactory)
        {
            StateFactory = stateFactory;
            return this;
        }

        IConfigureEventPublisher IConfigureThreadSafety.WithThreadSafetySelector(ThreadSafetySelector threadSafetySelector)
        {
            ThreadSafetySelector = threadSafetySelector;
            return this;
        }

        IManuallyConfigureEventAppliers IConfigureEventPublisher.WithEventPublisher(IPublishEvents publisher)
        {
            EventPublisher = publisher;
            return new ApplierConfigurationBuilder(this);
        }

        IBuildConfiguration IConfigureEventAppliers.WithEventApplier(IApplyEventsToStates eventApplier)
        {
            EventApplier = eventApplier;
            StateFactory.WarmupWith(eventApplier.SupportedStateTypes);
            return this;
        }

        IHoldAllConfiguration IBuildConfiguration.Build() => this;
    }
}
