namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    using AggregateBasedViewing = BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate.ViewingAggregateRoot;
    using RepoBasedViewing = BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate.ViewingAggregateRoot;

    [ShortRunJob]
    public class SaveChildEntityBenchmark
    {
        private AggregateFixture fixture = new AggregateFixture("testing");
        private static readonly RepoBasedViewing RepoBasedViewing = new RepoBasedViewing();

        [Params(1,10,100)]
        public int Capacity { get; set; }

        [Benchmark]
        public void SaveRepoBasedAggregateWithChilds()
        {
            var id = new ViewingId(Guid.NewGuid().ToString(), fixture.dateOfViewing, fixture.cinemaId);

            using (var session = fixture.ViewingFunctionalRepo.Load(id))
            {
                session.AddToStream(RepoBasedViewing.CreateViewing(id.CinemaId, id.MovieName, id.ShowingDate, Capacity));
                session.SaveChanges().Wait();
            }
        }

        [Benchmark]
        public void SaveAggregateBasedAggregatesithChilds()
        {
            //This will implicitly create childs as per capacity
            var aggregate = new AggregateBasedViewing(Capacity, fixture.dateOfViewing, Guid.NewGuid().ToString(),
                fixture.cinemaId);

            fixture.ViewingAggregateRepository.Save(aggregate).Wait();
        }
    }
}
