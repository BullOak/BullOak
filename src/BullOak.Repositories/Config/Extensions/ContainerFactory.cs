using System;
using BullOak.Repositories.StateEmit;

namespace BullOak.Repositories.Config
{
    using System.Collections.Generic;

    internal class ContainerFactory : ICreateStateInstances
    {
        private Func<Type, Func<object>> container;

        public ContainerFactory(Func<Type, Func<object>> container)
            => this.container = container ?? throw new ArgumentNullException(nameof(container));

        public object GetState(Type type)
            => container(type)();

        public void WarmupWith(IEnumerable<Type> typesToCreateFactoriesFor)
        { }
    }
}