namespace BullOak.Repositories.Session
{
    using System;

    public sealed class BasicValidationError : IValidationError
    {
        public string Message { get; }

        public BasicValidationError(string reason)
            => Message = reason ?? throw new ArgumentNullException(nameof(reason));

        public Exception GetAsException()
            => new BusinessException(Message);

        public static implicit operator BasicValidationError(string reason)
            => new BasicValidationError(reason);
    }
}
