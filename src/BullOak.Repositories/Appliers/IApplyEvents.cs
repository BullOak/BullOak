namespace BullOak.Repositories.Appliers
{
    using System;

    internal interface IApplyEventsInternal
    {
        bool CanApplyEvent(System.Type stateType, System.Type eventType);
        object Apply(object state, object @event);
    }

    public interface IApplyEvents<TState>
    {
        bool CanApplyEvent(System.Type eventType);
        TState Apply(TState state, object @event);
    }

    public interface IApplyEvent<TState, in TEvent>
    {
        TState Apply(TState state, TEvent @event);
    }

    internal class FunctionalInternalApplier : IApplyEventsInternal
    {
        private Func<object, object, object> applyFunction;
        private Func<Type, Type, bool> canApplyEventToStateFunction;

        public FunctionalInternalApplier(Func<object, object, object> applyFunction,
            Func<Type, Type, bool> canApplyEventToStateFunction)
        {
            this.applyFunction = applyFunction;
            this.canApplyEventToStateFunction = canApplyEventToStateFunction;
        }

        public object Apply(object state, object @event) => applyFunction(state, @event);

        public bool CanApplyEvent(Type stateType, Type eventType)
            => canApplyEventToStateFunction(stateType, eventType);

        public static IApplyEventsInternal From<TState>(IApplyEvents<TState> publicApplier)
        {
            var stateType = typeof(TState);

            return new FunctionalInternalApplier((s, e) => publicApplier.Apply((TState) s, e),
                (s, e) => s == stateType && publicApplier.CanApplyEvent(e));
        }

        public static IApplyEventsInternal From<TState, TEvent>(IApplyEvent<TState, TEvent> publicApplier)
        {
            var stateType = typeof(TState);
            var eventType = typeof(TEvent);

            return new FunctionalInternalApplier((s, e) => publicApplier.Apply((TState) s, (TEvent) e),
                (s, e) => s == stateType && (e == eventType || e.IsSubclassOf(eventType)));
        }

        public static Func<IApplyEventsInternal> From<TState>(Func<IApplyEvents<TState>> publicApplierFactory)
        {
            var stateType = typeof(TState);

            return () =>
            {
                var applier = publicApplierFactory();

                return new FunctionalInternalApplier((s, e) => applier.Apply((TState) s, e),
                    (s, e) => s == stateType && applier.CanApplyEvent(e));
            };
        }

        public static Func<IApplyEventsInternal> From<TState, TEvent>(Func<IApplyEvent<TState, TEvent>> publicApplierFactory)
        {
            var stateType = typeof(TState);
            var eventType = typeof(TEvent);


            return () =>
            {
                var applier = publicApplierFactory();

                return new FunctionalInternalApplier((s, e) => applier.Apply((TState) s, (TEvent) e),
                    (s, e) => s == stateType && (e == eventType || e.IsSubclassOf(eventType)));
            };
        }
    }
}