namespace BullOak.Repositories.EntityFramework.Test.Integration.Contexts
{
    using System;
    using TechTalk.SpecFlow;

    internal class ClientIdContainer
    {
        private static readonly string IdKey = Guid.NewGuid().ToString();
        private ScenarioContext ScenarioContext;

        public string Id
        {
            get => (string) ScenarioContext[IdKey];
            set => ScenarioContext[IdKey] = value;
        }

        public string Client2Id
        {
            get => (string) ScenarioContext[IdKey + nameof(Client2Id)];
            set => ScenarioContext[IdKey + nameof(Client2Id)] = value;
        }

        public string Client3Id
        {
            get => (string) ScenarioContext[IdKey + nameof(Client3Id)];
            set => ScenarioContext[IdKey + nameof(Client3Id)] = value;
        }

        public int Revision
        {
            get => (int) ScenarioContext[IdKey + nameof(Revision)];
            set => ScenarioContext[IdKey + nameof(Revision)] = value;
        }

        public ClientIdContainer(ScenarioContext scenarioContext)
            => ScenarioContext = scenarioContext;

        public void ResetToNew()
        {
            Id = Guid.NewGuid().ToString();
            Client2Id = Guid.NewGuid().ToString();
            Client3Id = Guid.NewGuid().ToString();
            Revision = 0;
        }
    }
}