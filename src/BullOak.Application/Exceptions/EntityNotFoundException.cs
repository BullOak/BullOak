namespace BullOak.Application.Exceptions
{
    using System;

    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityId, Type typeOfEntity, Exception innerException = null)
            : base($"Entity {typeOfEntity.Name} with id {entityId} was not found", innerException)
        { }
    }
}
