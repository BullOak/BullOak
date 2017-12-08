namespace BullOak.Repositories.StateEmit
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;

    internal class EmittedTypeFactory : ICreateStateInstances
    {
        private static ConcurrentDictionary<Type, Func<object>> TypeFactories = new ConcurrentDictionary<Type, Func<object>>();

        public object GetState(Type type)
        {
            if (TypeFactories.TryGetValue(type, out var factory)) return factory();

            lock (TypeFactories)
            {
                if (!TypeFactories.ContainsKey(type))
                {
                    TypeFactories[type] = CreateTypeFactoryMethod(type);
                }
            }

            return TypeFactories[type]();
        }

        private static Func<object> CreateTypeFactoryMethod(Type type)
        {
            var typeToCreate = type.IsInterface ? StateTypeEmitter.EmitType(type) : type;

            var ctor = typeToCreate.GetConstructor(Type.EmptyTypes);

            if (ctor == null || !ctor.IsPublic)
                throw new ArgumentException("Requested type has to have a default ctor", nameof(type));

            return Expression.Lambda<Func<object>>(Expression.New(ctor)).Compile();
        }
    }
}
