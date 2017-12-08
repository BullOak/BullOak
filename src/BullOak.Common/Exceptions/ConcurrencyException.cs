namespace BullOak.Common.Exceptions
{
    using System;

    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string aggregateId, Type aggregateType)
            : base($"Optimistic concurrency error when saving aggregate {aggregateType.Name} with id {aggregateId}")
        { }

        public ConcurrencyException(string message, object eventDetails = null) :
            base($"Optimistic concurrency error when saving {eventDetails}," + Environment.NewLine +
                 $"Exception Message: {message}")
        { }

        public ConcurrencyException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }
}
