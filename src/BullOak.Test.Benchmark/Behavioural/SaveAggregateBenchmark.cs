namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    [ShortRunJob]
    [MemoryDiagnoser]
    public class SaveAggregateBenchmark
    {
        private AggregateFixture fixture = new AggregateFixture("testing");
        private int capacity = 20;

        [Benchmark]
        public void SaveRepoBasedAggregate()
        {
            var cinemaId = new CinemaAggregateRootId(Guid.NewGuid().ToString());

            using (var session = fixture.CinemaFunctionalRepo.BeginSessionFor(cinemaId))
            {
                var @event = new BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate.CinemaAggregateRoot()
                    .Create(fixture.correlationId, capacity, Guid.NewGuid().ToString());

                session.AddEvent(@event);

                session.SaveChanges();
            }
        }

        //[Benchmark(Baseline = true)]
        public void SaveAnAggregateBasedAggregate()
        {
            var aggregate = new BullOak.Test.EndToEnd.Stub.AggregateBased.CinemaAggregate.CinemaAggregateRoot(fixture.correlationId, capacity, Guid.NewGuid().ToString());
            fixture.CinemaAggregateRepository.Save(aggregate).Wait();
        }

    }
}