namespace BullOak.Application.Exceptions
{
    using System;
    using Common.Exceptions;

    public class InvalidParameterException : BusinessException
    {
        public InvalidParameterException(Guid correlationId, string message) 
            : base(correlationId, message)
        { }
        public InvalidParameterException(Guid correlationId, string message, Exception ex) 
            : base(correlationId, message, ex)
        { }
    }
}
