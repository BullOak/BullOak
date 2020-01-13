namespace BullOak.Repositories.Session
{
    using System;

    public interface IValidationError
    {
        string Message { get; }
        Exception GetAsException();
    }
}
