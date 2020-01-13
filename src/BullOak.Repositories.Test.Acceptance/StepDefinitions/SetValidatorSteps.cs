using System;
using System.Collections.Generic;
using System.Text;

namespace BullOak.Repositories.Test.Acceptance.StepDefinitions
{
    using BullOak.Repositories.Session;
    using BullOak.Repositories.Test.Acceptance.Contexts;
    using TechTalk.SpecFlow;

    [Binding]
    internal class SetValidatorSteps
    {
        private readonly PassThroughValidator passThroughValidator;

        public SetValidatorSteps(PassThroughValidator passThroughValidator)
            => this.passThroughValidator = passThroughValidator;

        [Given(@"a validator which enforces that a state should never be above (.*)")]
        public void GivenAValidatorWhichEnforcesThatAStateShouldNeverBeAbove(int maxValue)
        {
            passThroughValidator.CurrentValidator = new MaxValueValidator(maxValue);
        }
    }

    internal class MaxValueValidator : IValidateState<IHoldHigherOrder>
    {
        private readonly int maxValue;

        public MaxValueValidator(int maxValue)
            => this.maxValue = maxValue;

        /// <inheritdoc />
        public ValidationResults Validate(IHoldHigherOrder state)
        {
            if (state.HigherOrder > maxValue)
                return ValidationResults.Errors(new BasicValidationError[] { $"LastBalance is more than expected {maxValue}" });

            return ValidationResults.Success();
        }
    }
}
