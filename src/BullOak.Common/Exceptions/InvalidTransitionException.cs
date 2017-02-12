namespace BullOak.Common.Exceptions
{
    using System;

    public class InvalidTransitionException : BusinessException
    {
        public InvalidTransitionException(Guid correlationId, string fromStatus, string toStatus)
            : base(correlationId, $"Invalid transition from {fromStatus} -> {toStatus}")
        {
        }
    }
}
