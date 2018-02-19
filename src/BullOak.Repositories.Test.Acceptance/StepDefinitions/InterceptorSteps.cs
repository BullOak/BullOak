namespace BullOak.Repositories.Test.Acceptance.StepDefinitions
{
    using BullOak.Repositories.Middleware;
    using BullOak.Repositories.Test.Acceptance.Contexts;
    using FluentAssertions;
    using TechTalk.SpecFlow;

    [Binding]
    public class InterceptorSteps
    {
        private readonly InterceptorContext interceptorContext;

        public InterceptorSteps(InterceptorContext interceptorContext)
            => this.interceptorContext = interceptorContext;

        [Then(@"the interceptor should be called")]
        public void ThenTheInterceptorShouldBeCalled()
        {
            interceptorContext.MethodsCalled.Should().Contain(nameof(IInterceptEvents.BeforePublish));
            interceptorContext.MethodsCalled.Should().Contain(nameof(IInterceptEvents.AfterPublish));
            interceptorContext.MethodsCalled.Should().Contain(nameof(IInterceptEvents.BeforeSave));
            interceptorContext.MethodsCalled.Should().Contain(nameof(IInterceptEvents.AfterSave));
        }
    }
}
