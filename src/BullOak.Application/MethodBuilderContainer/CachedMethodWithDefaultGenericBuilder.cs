namespace BullOak.Application.MethodBuilderContainer
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// CachedMethodWithDefaultGenericBuilder caches a method info.
    /// This will build the cached generic methods as per the types of the paramters provided to call the method.
    /// This is useful when we want to have the generic parameters available when we are reconstituting an
    /// aggregate. It is needed because we retrieve object references from our event store, which means that
    /// we lose type info, which is needed for performant reconstitution.
    /// </summary>
    internal class CachedMethodWithDefaultGenericBuilder : CachedMethodBase
    {
        private readonly ConcurrentDictionary<Type[], MethodInfo> builtCachedMethods;

        public CachedMethodWithDefaultGenericBuilder(MethodInfo mi)
            : base(mi)
        {
            builtCachedMethods = new ConcurrentDictionary<Type[], MethodInfo>(new TypeArrayEqualityComparer());
        }

        protected override MethodInfo GetAndBuildFor(object[] parameters)
        {
            if (!cachedMethod.IsGenericMethod || !cachedMethod.IsGenericMethodDefinition) return cachedMethod;

            var parameterTypes = parameters.Select(x => x.GetType()).ToArray();

            return builtCachedMethods.GetOrAdd(parameterTypes, types =>
            {
                var genericArgCount = cachedMethod.GetGenericArguments().Length;

                return cachedMethod.MakeGenericMethod(types.Take(genericArgCount).ToArray());
            });
        }
    }
}