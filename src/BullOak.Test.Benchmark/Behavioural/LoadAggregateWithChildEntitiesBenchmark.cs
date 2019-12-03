namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    using RepoBasedViewing = BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate.ViewingAggregateRoot;

    [ShortRunJob]
    public class LoadAggregateWithChildEntitiesBenchmark
    {
        private AggregateFixture fixture;

        private ViewingId viewingId;

        [Params(10, 100)]
        public int Capacity { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            fixture = new AggregateFixture(Guid.NewGuid().ToString());
            viewingId = new ViewingId(Guid.NewGuid().ToString(), fixture.dateOfViewing, fixture.cinemaId);
            fixture.AddViewingAndSeatCreationEvents(viewingId, Capacity);
            for (ushort u = 0; u < Capacity; u++)
                fixture.AddSeatReservationEvent(viewingId, u);
        }

        [Benchmark]
        public IViewingState LoadRepoBasedAggregateWithChilds()
        {
            using (var session = fixture.ViewingFunctionalRepo.BeginSessionFor(viewingId).Result)
            {
                return session.GetCurrentState();
            }
        }
    }
}
