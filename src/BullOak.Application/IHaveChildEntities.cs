namespace BullOak.Application
{
    using System;
    using BullOak.Common;

    public interface IHaveChildEntities<TChild, TChildId> where TChildId : IId, IEquatable<TChildId>
    {
        TChild GetOrAdd(TChildId id, Func<TChildId, TChild> factory);
    }
}