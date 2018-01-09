namespace BullOak.Repositories.Config
{
    using System;
    using BullOak.Repositories.StateEmit;

    internal class ContainerFactory : BaseTypeFactory
    {
        private Func<Type, Func<object>> container;

        public ContainerFactory(Func<Type, Func<object>> container)
            => this.container = container ?? throw new ArgumentNullException(nameof(container));

        public override object GetState(Type type)
            => container(type)();
    }
}