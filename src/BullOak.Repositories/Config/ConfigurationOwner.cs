using System;
using System.Collections.Generic;

//The above are outside so that the usings inside to be easier to read\write
namespace BullOak.Repositories
{
    using System.Reflection;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.Config;
    using BullOak.Repositories.EventPublisher;
    using BullOak.Repositories.Middleware;
    using BullOak.Repositories.Rehydration;
    using BullOak.Repositories.StateEmit;
    using BullOak.Repositories.Upconverting;

    using StateTypeToCollectionTypeSelector = Func<Type, Func<ICollection<ItemWithType>>>;
    using ThreadSafetySelector = Func<Type, bool>;

    public interface IConfigureUpconverter : IConfigureBullOak
    {
        IBuildConfiguration WithUpconverter(IUpconvertStoredItems upconverter);
    }

    public interface IConfigureUpconverters
    {
        IConfigureUpconverters WithUpconvertersFrom(Assembly assembly);
        IBuildConfiguration AndNoMoreUpconverters();
    }

    public interface IConfigureBullOak
    {
        void AddInterceptor(IInterceptEvents interceptor);
    }

    public class ConfigurationOwner : IConfigureEventCollectionType, IConfigureStateFactory, IConfigureThreadSafety, IConfigureEventPublisher,
        IHoldAllConfiguration, IBuildConfiguration, IConfigureEventAppliers, IConfigureUpconverter
    {
        public StateTypeToCollectionTypeSelector CollectionTypeSelector { get; private set; }
        public IPublishEvents EventPublisher { get; private set; }
        public IApplyEventsToStates EventApplier { get; private set; }
        public IRehydrateState StateRehydrator { get; private set; }
        public ThreadSafetySelector ThreadSafetySelector { get; private set; }
        public ICreateStateInstances StateFactory { get; private set; }
        public IUpconvertStoredItems EventUpconverter { get; private set; }
        public List<IInterceptEvents> InterceptorList { get; } = new List<IInterceptEvents>();
        public IInterceptEvents[] Interceptors { get; private set; }
        public bool HasInterceptors { get; private set; } = false;

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

        IConfigureUpconverter IConfigureEventAppliers.WithEventApplier(IApplyEventsToStates eventApplier)
        {
            EventApplier = eventApplier;
            StateFactory.WarmupWith(eventApplier.SupportedStateTypes);
            return this;
        }

        void IConfigureBullOak.AddInterceptor(IInterceptEvents interceptor)
        {
            this.InterceptorList.Add(interceptor);
        }

        IBuildConfiguration IConfigureUpconverter.WithUpconverter(IUpconvertStoredItems upconverter)
        {
            this.EventUpconverter = upconverter ?? throw new ArgumentNullException(nameof(upconverter)
                , $"A non-null upconverter is required. Please use one of the extension methods to set them up. See {nameof(UpconverterExtensions.WithNoUpconverters)} or {nameof(UpconverterExtensions.WithUpconvertersFrom)}");

            return this;
        }

        IHoldAllConfiguration IBuildConfiguration.Build()
        {
            if (InterceptorList.Count > 0)
            {
                Interceptors = InterceptorList.ToArray();
                HasInterceptors = true;
            }

            this.StateRehydrator = new Rehydrator(this);

            return this;
        }
    }
}
