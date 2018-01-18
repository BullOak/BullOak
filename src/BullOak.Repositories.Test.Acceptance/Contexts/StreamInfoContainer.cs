namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;
    using TechTalk.SpecFlow;

    internal class StreamInfoContainer
    {
        private static readonly string IdKey = Guid.NewGuid().ToString();
        private ScenarioContext ScenarioContext;

        public string Id
        {
            get => (String) ScenarioContext[IdKey];
            set => ScenarioContext[IdKey] = value;
        }

        public string Stream2Id
        {
            get => (String) ScenarioContext[IdKey + nameof(Stream2Id)];
            set => ScenarioContext[IdKey + nameof(Stream2Id)] = value;
        }

        public string Stream3Id
        {
            get => (String) ScenarioContext[IdKey + nameof(Stream3Id)];
            set => ScenarioContext[IdKey + nameof(Stream3Id)] = value;
        }

        public int Revision
        {
            get => (int) ScenarioContext[IdKey + nameof(Revision)];
            set => ScenarioContext[IdKey + nameof(Revision)] = value;
        }

        public StreamInfoContainer(ScenarioContext scenarioContext)
            => ScenarioContext = scenarioContext;

        public void ResetToNew()
        {
            Id = "StreamId_" + Guid.NewGuid().ToString();
            Stream2Id = "StreamId_" + Guid.NewGuid().ToString();
            Stream3Id = "StreamId_" + Guid.NewGuid().ToString();
            Revision = 0;
        }
    }
}
