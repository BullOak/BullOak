namespace BullOak.Application.Exceptions
{
    using System;

    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException(string aggregateId, Type aggregateType, Exception innerException = null)
            : base($"Aggregate {aggregateType.Name} with id {aggregateId} was not found", innerException)
        { }
    }
}
