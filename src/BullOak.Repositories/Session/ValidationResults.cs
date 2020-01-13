namespace BullOak.Repositories.Session
{
    using System.Collections.Generic;

    public struct ValidationResults
    {
        public bool IsSuccess { get; }
        public IEnumerable<IValidationError> ValidationErrors { get; }

        private ValidationResults(bool isSuccess, IEnumerable<IValidationError> validationErrors)
        {
            IsSuccess = isSuccess;
            ValidationErrors = validationErrors ?? new IValidationError[0];
        }

        public static ValidationResults Success()
            => new ValidationResults(true, null);

        public static ValidationResults Errors(IEnumerable<IValidationError> validationErrors)
            => new ValidationResults(false, validationErrors);
    }
}
