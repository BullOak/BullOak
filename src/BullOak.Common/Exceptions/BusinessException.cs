namespace BullOak.Common.Exceptions
{
    using System;

    public class BusinessException : Exception
    {
        public Guid CorrelationId { get; set; }

        public BusinessException(Guid correlationId, string message, Exception ex = null) 
            : base(message, ex)
        {
            CorrelationId = correlationId;
        }
    }
}
