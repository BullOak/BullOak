namespace BullOak.Repositories
{
    using System;

    public struct ItemWithType
    {
        public readonly Type type;
        public readonly object instance;

        public ItemWithType(object instance)
        {
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
            this.type = instance.GetType();
        }

        public ItemWithType(object instance, Type type)
        {
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
            this.type = type ?? throw new ArgumentNullException(nameof(type));

            if (!type.IsInstanceOfType(instance))
                throw new ArgumentException($"The provided instance was of type {instance.GetType()} but it must be subclass of or implement type {type.Name}");
        }
    }
}