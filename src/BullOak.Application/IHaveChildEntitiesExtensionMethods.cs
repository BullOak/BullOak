namespace BullOak.Application
{
    using System;
    using BullOak.Common;
    using Exceptions;
    using Messages;

    public static class HaveChildEntitiesExtensionMethods
    {
        public static TChild GetOrCreate<TChild, TChildId>(this IHaveChildEntities<TChild, TChildId> parent, TChildId id)
            where TChild : Entity<TChildId>, new()
            where TChildId : IId, IEquatable<TChildId>
        {
            return parent.GetOrAdd(id, identity =>
            {
                var newChild = new TChild();

                var childAsHasParent = newChild as IHaveAParent;
                childAsHasParent?.SetParent(parent as Entity);

                return newChild;
            });
        }

        public static TChild GetOrThrow<TChild, TChildId>(this IHaveChildEntities<TChild, TChildId> parent, TChildId id, Func<Exception> exceptionFactory = null)
            where TChild : Entity<TChildId>, new()
            where TChildId : IId, IEquatable<TChildId>
        {
            return parent.GetOrAdd(id, identity =>
            {
                throw exceptionFactory?.Invoke() ?? new EntityNotFoundException(id.ToString(), typeof(TChild));
            });
        }
    }
}
