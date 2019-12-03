namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

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

            using (var session = fixture.ViewingFunctionalRepo.BeginSessionFor(id).Result)
            {
                session.AddEvent(RepoBasedViewing.CreateViewing(id.CinemaId, id.MovieName, id.ShowingDate, Capacity));
                session.SaveChanges().Wait();
            }
        }
    }
}
