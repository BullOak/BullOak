namespace BullOak.Application.Exceptions
{
    using System;
    using BullOak.Common.Exceptions;

    public class InvalidStateChangeException : BusinessException
    {
        public InvalidStateChangeException(Guid correlationId, string message) 
            : base(correlationId, message)
        { }
        public InvalidStateChangeException(Guid correlationId, string message, Exception ex) 
            : base(correlationId, message, ex)
        { }
    }
}
