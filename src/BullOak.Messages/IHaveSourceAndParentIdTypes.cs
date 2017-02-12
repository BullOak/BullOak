namespace BullOak.Messages
{
    using System;

    public interface IHaveSourceAndParentIdTypes
    {
        Type SourceIdType { get; }
        Type ParentIdType { get; }
    }
}