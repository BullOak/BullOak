namespace BullOak.Application.Exceptions
{
    using System;

    public class EntityExistsException : Exception
    {
        public string EntityId { get; private set; }
        public Type EntityType { get; private set; }
        public object RootId { get; private set; }

        public EntityExistsException(string entityId, Type typeOfEntity, object rootId, Exception innerException = null)
            : base(
                $"Entity {typeOfEntity.Name} with Id {entityId} already exists within aggregate root of Id {rootId}",
                innerException)
        {
            EntityId = entityId;
            EntityType = typeOfEntity;
            RootId = rootId;
        }
    }
}
