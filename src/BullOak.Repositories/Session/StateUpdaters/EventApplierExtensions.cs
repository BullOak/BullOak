namespace BullOak.Repositories.Session.StateUpdaters
{
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.Appliers;

    internal static class EventApplierExtensions
    {
        public static TState ApplyEvents<TState>(this IApplyEventsToStates eventApplier, TState state,
            IEnumerable<object> events)
            => events.Aggregate(state, eventApplier.Apply);
    }
}
