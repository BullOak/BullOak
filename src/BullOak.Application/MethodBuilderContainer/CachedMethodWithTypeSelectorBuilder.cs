namespace BullOak.Application.MethodBuilderContainer
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// CachedMethodWithTypeSelectorBuilder caches a method info along with type selectors for generic methods.
    /// This will build the cached generic methods as per the provided generic type selector.
    /// This is useful when we want to have the generic parameters available when we are reconstituting an
    /// aggregate. It is needed because we retrieve object references from our event store, which means that
    /// we lose type info, which is needed for performant reconstitution.
    /// </summary>
    internal class CachedMethodWithTypeSelectorBuilder : CachedMethodBase
    {
        private readonly ConcurrentDictionary<Type[], MethodInfo> builtCachedMethods;
        private readonly Func<object[], Type[]> typeSelector;

        public CachedMethodWithTypeSelectorBuilder(MethodInfo mi, Func<Type[], Type[]> genericTypeSelector)
            : base(mi)
        {
            builtCachedMethods = new ConcurrentDictionary<Type[], MethodInfo>(new TypeArrayEqualityComparer());
            typeSelector = objs => genericTypeSelector(objs.Select(x => x.GetType()).ToArray());
        }

        public CachedMethodWithTypeSelectorBuilder(MethodInfo mi, Func<object[], Type[]> typeSelector)
            : base(mi)
        {
            builtCachedMethods = new ConcurrentDictionary<Type[], MethodInfo>(new TypeArrayEqualityComparer());
            this.typeSelector = typeSelector;
        }

        protected override MethodInfo GetAndBuildFor(object[] parameters)
        {
            if (!cachedMethod.IsGenericMethod || !cachedMethod.IsGenericMethodDefinition) return cachedMethod;

            return builtCachedMethods.GetOrAdd(typeSelector(parameters),
                types => cachedMethod.MakeGenericMethod(typeSelector(parameters)));
        }
    }
}