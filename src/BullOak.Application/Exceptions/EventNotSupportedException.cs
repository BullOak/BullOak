namespace BullOak.Application.Exceptions
{
    using System;

    public class EventNotSupportedException : Exception
    {
        public EventNotSupportedException(object entityId, Type typeOfEntity, Type typeOfEvent)
            : base($"Entity {typeOfEntity.Name} with Id {entityId} could not process event {typeOfEvent.Name}")
        { }
    }
}
