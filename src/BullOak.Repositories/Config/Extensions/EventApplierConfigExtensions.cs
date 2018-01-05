namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;
    using BullOak.Repositories.Appliers;
    using System.Linq;
    using System.Reflection;

    public static class EventApplierConfigExtensions
    {
        private static readonly Type openApplierType = typeof(IApplyEvents<>);

        private static readonly Type openEventApplierType = typeof(IApplyEvent<,>);

        public static IManuallyConfigureEventAppliers WithEventApplier<TState, TEvent>(this IManuallyConfigureEventAppliers self, Func<TState, TEvent, TState> stateApplier)
            => self.WithEventApplier((FuncEventApplier<TState, TEvent>)stateApplier);
        public static IManuallyConfigureEventAppliers WithEventApplier<TState, TEvent>(this IManuallyConfigureEventAppliers self, IApplyEvent<TState, TEvent> stateApplier)
            => self.WithEventApplier((FuncEventApplier<TState, TEvent>)stateApplier.Apply);

        public static IManuallyConfigureEventAppliers WithAnyAppliersFromInstances(
            this IManuallyConfigureEventAppliers self,
            IEnumerable<object> appliers)
        {
            var applierList = appliers?.ToList() ?? new List<object>();

            if(applierList.Any(x=> x == null))
                throw new ArgumentNullException(nameof(appliers), "The list of appliers may not contain a null applier.");

            var applyEventInterfaceDefinitions = applierList.Select(x => new {Type = x.GetType(), Instance = x})
                .SelectMany(x => x.Type.GetInterfaces().Select(i => new {Interface = i, x.Instance}))
                .Where(x => x.Interface.IsGenericType)
                .Where(x => x.Interface.GetGenericTypeDefinition() == openApplierType ||
                            x.Interface.GetGenericTypeDefinition() == openEventApplierType);

            foreach (var applierTypeInfo in applyEventInterfaceDefinitions)
            {
                if (applierTypeInfo.Interface.GetGenericTypeDefinition() == openApplierType)
                {
                    //We have IApplyEvents<>
                    var stateType = applierTypeInfo.Interface.GetGenericArguments()[0];
                    self.WithEventApplier(stateType, applierTypeInfo.Instance);
                }
                else if (applierTypeInfo.Interface.GetGenericTypeDefinition() == openEventApplierType)
                {
                    //We have IApplyEvent<,>
                    var stateType = applierTypeInfo.Interface.GetGenericArguments()[0];
                    var eventType = applierTypeInfo.Interface.GetGenericArguments()[1];
                    self.WithEventApplier(stateType, eventType, applierTypeInfo.Instance);
                }
            }

            return self;
        }

        public static IManuallyConfigureEventAppliers WithAnyAppliersFrom(
            this IManuallyConfigureEventAppliers self,
            Assembly assemblyToAnalyze)
        {
            var appliers = assemblyToAnalyze.DefinedTypes
                .SelectMany(x => x.GetInterfaces(), (o, i) => new {Original = o, ImplementedInterface = i})
                .Where(x => x.ImplementedInterface.IsGenericType)
                .Where(x => x.ImplementedInterface.GetGenericTypeDefinition() == openApplierType || x.ImplementedInterface.GetGenericTypeDefinition() == openEventApplierType)
                .Distinct()
                .Select(x => Activator.CreateInstance(x.Original));

            self.WithAnyAppliersFromInstances(appliers);
            return self;
        }
    }
}
