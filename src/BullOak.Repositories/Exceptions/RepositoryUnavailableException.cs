namespace BullOak.Repositories.Exceptions
{
    using System;

    public class RepositoryUnavailableException : Exception
    {
        public RepositoryUnavailableException(string message) : base(message)
        {

        }


        public RepositoryUnavailableException(string message, Exception innerException) : base(message, innerException)
        {

        }


    }
}