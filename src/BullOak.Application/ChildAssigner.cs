namespace BullOak.Application
{
    using System;
    using BullOak.Common;
    using Exceptions;

    internal static class ChildAssigner
    {
        public static void TrySetChildInParent<TChild, TChildId>(object parent, TChild child)
            where TChild : Entity<TChildId>
            where TChildId : IId, IEquatable<TChildId>
        {
            if(parent == null) throw new ArgumentNullException(nameof(parent));

            var parentAsHaveChildren = parent as IHaveChildEntities<TChild, TChildId>;

            var childInParent = parentAsHaveChildren?.GetOrAdd(child.Id, _ => child);

            if (child != childInParent)
            {
                //We know here that parent implements IHaveAnId since all Entity<T> do and we do not
                // expose any non-internal ctors of non-generic Entity.
                throw new EntityExistsException(child.Id.ToString(), typeof(TChild), ((dynamic)parent).Id.ToString());
            }
        }
    }
}