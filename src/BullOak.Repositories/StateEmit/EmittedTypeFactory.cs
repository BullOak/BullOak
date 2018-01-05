namespace BullOak.Repositories.StateEmit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    //public class FactoryAlreadyExistsException : Exception
    //{
    //    public FactoryAlreadyExistsException(string message)
    //        :base(message)
    //    { }
    //}

    internal class EmittedTypeFactory : ICreateStateInstances
    {
        private ConcurrentDictionary<Type, Func<object>> TypeFactories = new ConcurrentDictionary<Type, Func<object>>();

        public void WarmupWith(IEnumerable<Type> typesToCreateFactoriesFor)
        {
            lock (TypeFactories)
            {
                foreach(var type in typesToCreateFactoriesFor)
                {
                    if (!TypeFactories.ContainsKey(type))
                    {
                        TypeFactories[type] = CreateTypeFactoryMethod(type);
                    }
                    //else
                    //    throw new FactoryAlreadyExistsException($"Factory for {type.Name} already exists and cannot be replaced.");
                }
            }
        }

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
