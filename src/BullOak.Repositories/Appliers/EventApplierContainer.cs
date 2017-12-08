namespace BullOak.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BullOak.Repositories.Appliers;

    public class EventApplierContainer
    {
        private readonly Dictionary<Type, List<object>> container = new Dictionary<Type, List<object>>();

        private class Resolver : ICreateEventAppliers
        {
            private readonly Dictionary<Type, List<object>> container = new Dictionary<Type, List<object>>();

            public Resolver(Dictionary<Type, List<object>> container)
                => this.container = container ?? throw new ArgumentNullException(nameof(container));

            public IEnumerable<IApplyEvents<TState>> GetInstance<TState>()
            {
                var key = typeof(TState);

                if (!container.TryGetValue(key, out List<object> handlers))
                    return new List<IApplyEvents<TState>>(0);

                return handlers.Cast<IApplyEvents<TState>>();
            }
        }

        public void Register<TState>(IApplyEvents<TState> applier)
        {
            lock (container)
            {
                //TODO: Fix this method
                var key = typeof(TState);

                if (!container.ContainsKey(key)) container[typeof(TState)] = new List<object>();

                (container[key] as List<object>).Add(applier);
            }
        }

        public void Register<TState, TEvent>(Func<TState, TEvent, TState> applier)
            => Register((FuncEventApplier<TState, TEvent>)applier);

        public ICreateEventAppliers Build()
        {
            lock (container)
            {
                return new Resolver(container.ToDictionary(x => x.Key, x => x.Value));
            }
        }
    }
}
