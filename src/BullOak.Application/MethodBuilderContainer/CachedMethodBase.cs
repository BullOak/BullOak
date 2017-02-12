namespace BullOak.Application.MethodBuilderContainer
{
    using System;
    using System.Reflection;
    using Exceptions;
    using System.Linq;

    /// <summary>
    /// CachedMethodBase caches a method info and encapsulates the use of reflection to call a method.
    /// See child classes for more info on use.
    /// </summary>
    internal abstract class CachedMethodBase : ICacheMethods
    {
        protected readonly MethodInfo cachedMethod;

        public CachedMethodBase(MethodInfo mi)
        {
            if (mi == null) throw new ArgumentNullException(nameof(mi));

            cachedMethod = mi;
        }

        public object Invoke(object targetInstance, params object[] parameters)
        {
            MethodInfo mi = cachedMethod.IsGenericMethodDefinition
                ? GetAndBuildFor(parameters)
                : cachedMethod;

            try
            {
                return mi.Invoke(targetInstance, parameters);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is EntityExistsException)
            {
                var entityExistEx = (EntityExistsException) ex.InnerException;

                throw new EntityExistsException(entityExistEx.EntityId, entityExistEx.EntityType, entityExistEx.RootId,
                    entityExistEx);
            }
            catch (Exception ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }

        protected abstract MethodInfo GetAndBuildFor(object[] parameters);
    }
}
