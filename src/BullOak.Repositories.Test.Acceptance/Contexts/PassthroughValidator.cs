using System;
using System.Collections.Generic;
using System.Text;

namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using BullOak.Repositories.Session;

    internal class PassThroughValidator : IValidateState<IHoldHigherOrder>
    {
        public IValidateState<IHoldHigherOrder> CurrentValidator { get; set; }

        /// <inheritdoc />
        public ValidationResults Validate(IHoldHigherOrder state)
            => CurrentValidator.Validate(state);
    }
}
