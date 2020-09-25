namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;
    using BullOak.Repositories.InMemory;
    using BullOak.Repositories.Session;
    using TechTalk.SpecFlow;

    internal class InMemoryStoreSessionContainer : IDisposable
    {
        private static readonly string id = Guid.NewGuid().ToString();

        private InMemoryEventSourcedRepository<string, IHoldHigherOrder> repository;

        public IManageSessionOf<IHoldHigherOrder> LastSession { get; private set; }

        public InMemoryStoreSessionContainer()
        { }

        public void Setup(PassThroughValidator passThroughValidator, IHoldAllConfiguration configuration)
        {
            if (repository != null) throw new Exception($"{nameof(repository)} already setup");

            repository = new InMemoryEventSourcedRepository<string, IHoldHigherOrder>(passThroughValidator, configuration);
        }

        public IManageSessionOf<IHoldHigherOrder> StartSession(string streamId, DateTime? appliesAt = null)
        {
            LastSession = repository.BeginSessionFor(streamId, appliesAt: appliesAt).Result;

            return LastSession;
        }

        public (ItemWithType, DateTime)[] GetStream(string id)
            => repository[id];

        public void SaveStream(string id, (ItemWithType, DateTime)[] events)
            => repository[id] = events;

        public void Dispose()
        {
            LastSession?.Dispose();
        }
    }
}
