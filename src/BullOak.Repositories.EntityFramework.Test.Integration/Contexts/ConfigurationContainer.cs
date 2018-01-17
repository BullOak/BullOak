namespace BullOak.Repositories.EntityFramework.Test.Integration.Contexts
{
    using System;
    using System.Reflection;
    using BullOak.Repositories.Config;
    using TechTalk.SpecFlow;

    public class ConfigurationContainer
    {
        private static readonly string IdKey = Guid.NewGuid().ToString();
        private readonly ScenarioContext scenarioContext;

        public IHoldAllConfiguration Configuration
        {
            get => (IHoldAllConfiguration) scenarioContext[IdKey];
            private set => scenarioContext[IdKey] = value;
        }

        public ConfigurationContainer(ScenarioContext scenarioContext)
            => this.scenarioContext = scenarioContext;

        public void Setup()
            => Configuration = GetNewConfiguration();

        public static IHoldAllConfiguration GetNewConfiguration()
            => BullOak.Repositories.Configuration.Begin()
                .WithDefaultCollection()
                .WithDefaultStateFactory()
                .NeverUseThreadSafe()
                .WithNoEventPublisher()
                .WithAnyAppliersFrom(Assembly.GetExecutingAssembly())
                .AndNoMoreAppliers()
                .WithNoUpconverters()
                .Build();
    }
}
