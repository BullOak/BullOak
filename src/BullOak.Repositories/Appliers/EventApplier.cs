namespace BullOak.Repositories.Appliers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class EventApplier : IApplyEventsToStates
    {
        private Dictionary<Type, IReadOnlyCollection<object>> stateAppliersDictionary =
            new Dictionary<Type, IReadOnlyCollection<object>>();


        public void SeedWith(IDictionary<Type, ICollection<object>> appliersByStateType)
        {
            var stateAppliersDictionary = new Dictionary<Type, IReadOnlyCollection<object>>();

            foreach (var appliersKey in appliersByStateType.Keys)
            {
                var appliers = appliersByStateType[appliersKey];
                stateAppliersDictionary[appliersKey] = appliers.ToArray();
            }

            this.stateAppliersDictionary = stateAppliersDictionary;
        }

        public TState Apply<TState, TEvent>(TState state, TEvent @event)
        {
            var typeOfState = typeof(TState);
            var typeOfEvent = typeof(TEvent);

            //TODO (Savvas): There may be a lot of room for optimization here by caching collection of appliers for a state type.
            if (stateAppliersDictionary.TryGetValue(typeOfState, out var applierCollection)) throw new ApplierNotFoundException(typeOfState);

            var applier = applierCollection
                .Cast<IApplyEvents<TState>>()
                .FirstOrDefault(x=> x.CanApplyEvent(@event));

            if (applier == null) throw new ApplierNotFoundException(typeOfState, typeOfEvent);

            return applier.Apply(state, @event);
        }
    }

    internal class ApplierNotFoundException : Exception
    {
        public ApplierNotFoundException(Type typeOfState, Type typeOfEvent)
            : base($"Applier for event {typeOfEvent.Name} for state {typeOfState.Name} was not found or registered.")
        { }
        public ApplierNotFoundException(Type typeOfState)
            : base($"No appliers where found for state {typeOfState.Name}.")
        { }
    }
}
