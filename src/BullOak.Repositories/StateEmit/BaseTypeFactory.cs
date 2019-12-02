namespace BullOak.Repositories.StateEmit
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using BullOak.Repositories.StateEmit.Emitters;

    internal abstract class BaseTypeFactory : ICreateStateInstances
    {
        private ConcurrentDictionary<Type, WrapperCreationResult> WrapperFactories
            = new ConcurrentDictionary<Type, WrapperCreationResult>();

        public virtual void WarmupWith(IEnumerable<Type> typesToCreateFactoriesFor)
        {
            lock (WrapperFactories)
            {
                foreach (var type in typesToCreateFactoriesFor)
                {
                    if (!WrapperFactories.ContainsKey(type))
                    {
                        var result = GetWrapper(type, false);
                        if(result.FactoryCreated) WrapperFactories[type] = result;
                    }
                }
            }
        }

        public Func<T, T> GetWrapper<T>()
            => (Func<T, T>)GetWrapper(typeof(T), true).Factory;

        public abstract object GetState(Type type);

        private WrapperCreationResult GetWrapper(Type type, bool throwExceptionIfCannotCreate = true)
        {
            if (WrapperFactories.TryGetValue(type, out var factory) && !throwExceptionIfCannotCreate) return factory;

            lock (WrapperFactories)
            {
                if (!WrapperFactories.ContainsKey(type))
                {
                    WrapperFactories[type] = CreateWrapperFactoryMethor(type, throwExceptionIfCannotCreate);
                }
            }

            return WrapperFactories[type];
        }

        private struct WrapperCreationResult
        {
            public object Factory { get; set; }
            public bool FactoryCreated { get; set; }
        }
        private static WrapperCreationResult CreateWrapperFactoryMethor(Type type, bool throwException = true)
        {
            if (!type.IsInterface)
            {
                if (throwException) throw new TypeCannotBeWrappedException(type);
                else
                    return new WrapperCreationResult
                    {
                        Factory = null,
                        FactoryCreated = false,
                    };
            }

            var wrapperType = StateTypeEmitter.EmitType(type, new StateWrapperEmitter());

            var ctor = wrapperType.GetConstructor(new[] { type });

            if (ctor == null || !ctor.IsPublic)
                throw new ArgumentException("Requested type has to have a default ctor", nameof(type));

            var objectStateParam = ParameterExpression.Parameter(type, "wrapped");

            var ctorCall = Expression.New(ctor, objectStateParam);

            return new WrapperCreationResult
            {
                Factory = Expression.Lambda(typeof(Func<,>).MakeGenericType(type, type),
                    ctorCall, objectStateParam).Compile(),
                FactoryCreated = true,
            };
        }
    }
}
