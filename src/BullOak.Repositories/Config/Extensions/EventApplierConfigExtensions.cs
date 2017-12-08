namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;
    using BullOak.Repositories.Appliers;
    using System.Linq;

    public static class EventApplierConfigExtensions
    {
        public static IManuallyConfigureEventAppliers WithEventApplier<TState, TEvent>(this IManuallyConfigureEventAppliers self, Func<TState, TEvent, TState> stateApplier)
            => self.WithEventApplier<TState>((FuncEventApplier<TState, TEvent>)stateApplier);
        public static IManuallyConfigureEventAppliers WithEventApplier<TState, TEvent>(this IManuallyConfigureEventAppliers self, IApplyEvent<TState, TEvent> stateApplier)
            => self.WithEventApplier<TState>((FuncEventApplier<TState, TEvent>)stateApplier.Apply);
        public static IManuallyConfigureEventAppliers WithAnyAppliersFrom(this IManuallyConfigureEventAppliers self, IEnumerable<object> appliers)
        {
            var applyEventInterfaceDefinitions = appliers.Select(x => new { Type = x.GetType(), Instance = x })
                .SelectMany(x => x.Type.GetInterfaces().Select(i => new { Interface = i, Instance = x }))
                .Where(x => x.Interface.IsGenericTypeDefinition && x.Interface.GetGenericTypeDefinition() == typeof(IApplyEvents<>));

            foreach (var applier in applyEventInterfaceDefinitions)
            {
                self = WithEventApplier(self, (dynamic)applier);
            }

            return self;
        }
    }
}
