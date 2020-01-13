namespace BullOak.Repositories.Session
{
    internal class AlwaysPassValidator<T> : IValidateState<T>
    {
        /// <inheritdoc />
        public ValidationResults Validate(T state)
            => ValidationResults.Success();
    }
}
