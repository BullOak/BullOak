namespace BullOak.Repositories
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using BullOak.Repositories.Appliers;

    internal class ApplierConfigurationBuilder : IManuallyConfigureEventAppliers, IBuildConfiguration
    {
        private readonly IConfigureEventAppliers baseConfiguration;
        private readonly ConcurrentDictionary<Type, ICollection<object>> applierCollection;

        public ApplierConfigurationBuilder(IConfigureEventAppliers baseConfiguration)
        {
            this.baseConfiguration = baseConfiguration ?? throw new ArgumentNullException(nameof(baseConfiguration));

            applierCollection= new ConcurrentDictionary<Type, ICollection<object>>();
        }

        public IBuildConfiguration WithEventApplier(IApplyEventsToStates eventApplier)
            => applierCollection.Count == 0
                ? baseConfiguration.WithEventApplier(eventApplier)
                : throw new Exception(
                    $"Items already configured. Please either provide an instance of {nameof(IApplyEventsToStates)} or manually configure\add each {typeof(IApplyEvents<>).Name}");

        public IManuallyConfigureEventAppliers WithEventApplier<TState>(IApplyEvents<TState> stateApplier)
        {
            //applierCollection.Add(stateApplier);
            var key = typeof(TState);

            if (applierCollection.TryGetValue(key, out var stateAppliers))
            {
                lock (stateAppliers)
                {
                    stateAppliers.Add(stateApplier);
                }
                return this;
            }

            stateAppliers = applierCollection.GetOrAdd(key, new HashSet<object>());

            lock (stateAppliers)
            {
                stateAppliers.Add(stateAppliers);
            }

            return this;
        }

        public IBuildConfiguration AndNoMoreAppliers()
            => baseConfiguration.WithEventApplier(BuildEventApplierFrom(applierCollection));

        public IHoldAllConfiguration Build()
            => baseConfiguration
            .WithEventApplier(BuildEventApplierFrom(applierCollection))
            .Build();

        private static IApplyEventsToStates BuildEventApplierFrom(ConcurrentDictionary<Type, ICollection<object>> collection)
        {
            var applier = new EventApplier();
            applier.SeedWith(collection);
            return applier;
        }
    }
}