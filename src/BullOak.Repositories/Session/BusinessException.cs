namespace BullOak.Repositories.Session
{
    using System;

    public class BusinessException : Exception
    {
        public BusinessException(string message)
            :base(message)
        { }
    }
}
