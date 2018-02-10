namespace BullOak.Repositories.Exceptions
{
    using System;

    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string id, Exception innerException)
            : base($"Concurrency exception when trying to persist entity {id}", innerException)
        {
        }

        public ConcurrencyException(Type type, Exception innerException)
            : base($"Concurrency exception when trying to persist entity {type.FullName}", innerException)
        {
        }
    }
}
