namespace BullOak.Repositories.Test.Acceptance.Contexts
{
    using System;
    using System.Linq;
    using BullOak.Repositories.Appliers;
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
            => repository[id].Select(x => (x.Item1.ToItemWithType(), x.Item2)).ToArray();

        public void SaveStream(string id, (ItemWithType, DateTime)[] events)
        {
            var stream = new (StoredEvent, DateTime)[events.Length];

            for(int i = 0;i<events.Length;i++)
                stream[i] = (events[i].Item1.ToStoredEvent(i), events[i].Item2);

            repository[id] = stream;
        }

        public void Dispose()
        {
            LastSession?.Dispose();
        }
    }
}
