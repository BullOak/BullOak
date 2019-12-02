namespace BullOak.Repositories.Test.Acceptance.StepDefinitions
{
    using System;
    using BullOak.Repositories.Test.Acceptance.Contexts;
    using TechTalk.SpecFlow;

    public class LastAccessedStateContainer
    {
        private static readonly string id = Guid.NewGuid().ToString();
        private ScenarioContext scenarioContext;
        public IHoldHigherOrder LatestLoadedState
        {
            get => (IHoldHigherOrder) scenarioContext[id];
            set => scenarioContext[id] = value;
        }

        public LastAccessedStateContainer(ScenarioContext scenarioContext)
            => this.scenarioContext = scenarioContext;
    }
}
