namespace BullOak.Application.Exceptions
{
    using System;

    public class AggregateDeletedException : Exception
    {
        public AggregateDeletedException(string aggregateId, Type aggregateType, Exception innerException = null)
            : base($"Aggregate {aggregateType.Name} with id {aggregateId} has been deleted", innerException)
        { }
    }
}
