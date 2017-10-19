namespace BullOak.Test.Benchmark
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    using AggregateBasedViewing = BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate.ViewingAggregateRoot;
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
            fixture.AddViewingAndSeatCreatiuonEvents(viewingId, Capacity);
        }

        [Benchmark]
        public ViewingState LoadRepoBasedAggregateWithChilds()
        {
            using (var session = fixture.ViewingFunctionalRepo.Load(viewingId))
            {
                return session.State;
            }
        }

        [Benchmark]
        public AggregateBasedViewing LoadAggregateBasedAggregateWithChilds()
        {
            return fixture.ViewingAggregateRepository.Load(viewingId).Result;
        }
    }
}
