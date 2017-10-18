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
    public class EditChildEntitiesBenchmark
    {
        private AggregateFixture fixture;
        private static readonly RepoBasedViewing viewingAggregate = new RepoBasedViewing();

        private ViewingId viewingId;

        [Params(1, 10, 100)]
        public int Capacity { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            fixture = new AggregateFixture(Guid.NewGuid().ToString(), Capacity);
            viewingId = new ViewingId(Guid.NewGuid().ToString(), fixture.dateOfViewing, fixture.cinemaId);
            fixture.AddViewingAndSeatCreatiuonEvents(viewingId, Capacity);
        }

        [Benchmark]
        public void EditChildFromRepoBasedAggregate()
        {
            using (var session = fixture.ViewingFunctionalRepo.Load(viewingId))
            {
                var events = viewingAggregate.ReserveSeat(session.State, 0);

                session.AddToStream(events);
                session.SaveChanges().Wait();
            }

            var eventCount = fixture.ViewingFunctionalRepo[viewingId].Count;
            fixture.ViewingFunctionalRepo[viewingId].RemoveAt(eventCount - 1);
        }

        [Benchmark]
        public void EditChildFromAggregateBasedAggregate()
        {
            var aggregate = fixture.ViewingAggregateRepository.Load(viewingId).Result;

            aggregate.ReserveSeat(0);

            fixture.ViewingAggregateRepository.Save(aggregate).Wait();

            var eventCount = fixture.ViewingAggregateRepository[viewingId.ToString()].Count;
            fixture.ViewingAggregateRepository[viewingId.ToString()].RemoveAt(eventCount - 1);
        }
    }
}
