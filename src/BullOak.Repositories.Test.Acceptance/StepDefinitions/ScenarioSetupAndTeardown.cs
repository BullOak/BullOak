namespace BullOak.Repositories.Test.Acceptance.StepDefinitions
{
    using System;
    using System.Reflection;
    using BullOak.Repositories.Config;
    using BullOak.Repositories.InMemory;
    using BullOak.Repositories.Test.Acceptance.Contexts;
    using TechTalk.SpecFlow;

    [Binding]
    internal class ScenarioSetupAndTeardown
    {
        private readonly StreamInfoContainer streamInfoContainer;
        private readonly InMemoryStoreSessionContainer sessionContainer;
        private readonly InterceptorContext interceptorContext;
        private readonly PassThroughValidator passThroughValidator;

        public ScenarioSetupAndTeardown(StreamInfoContainer streamInfoContainer,
            InMemoryStoreSessionContainer sessionContainer,
            InterceptorContext interceptorContext,
            PassThroughValidator passThroughValidator)
        {
            this.streamInfoContainer = streamInfoContainer;
            this.sessionContainer = sessionContainer;
            this.interceptorContext = interceptorContext;
            this.passThroughValidator = passThroughValidator;
        }

        private bool isAlreadySetup = false;
        private void Set(Func<IConfigureUpconverter, IBuildConfiguration> upconverterSetup)
        {
            if (isAlreadySetup) return;
            streamInfoContainer.ResetToNew();

            var upconverterConfig = Configuration.Begin()
                .WithDefaultCollection()
                .WithDefaultStateFactory()
                .WithInterceptor(interceptorContext)
                .NeverUseThreadSafe()
                .WithNoEventPublisher()
                .WithAnyAppliersFrom(Assembly.GetExecutingAssembly())
                .AndNoMoreAppliers();

            var configuration = upconverterSetup(upconverterConfig)
                .Build();

            var tempInMemRepo = new InMemoryEventSourcedRepository<int, IHoldHigherOrder>(configuration);
            var defaultValidator = tempInMemRepo.StateValidator;
            passThroughValidator.CurrentValidator = defaultValidator;

            sessionContainer.Setup(passThroughValidator, configuration);
            isAlreadySetup = true;
        }

        [BeforeScenario(Order=10)]
        public void Setup()
            => Set(c => c.WithNoUpconverters());

        [BeforeScenario("WithBuyerNameUpconverter", Order = 1)]
        public void SetupWithBuyerNameUpconverter()
            => Set(c => c.WithUpconvertersFrom(typeof(BuyerNameUpconverter)).AndNoMoreUpconverters());

        [BeforeScenario("WithBalanceUpdateUpconverter", Order = 1)]
        public void SetupWithBalanceUpconverter()
            => Set(c => c.WithUpconvertersFrom(typeof(BalanceUpconverter)).AndNoMoreUpconverters());
    }
}
