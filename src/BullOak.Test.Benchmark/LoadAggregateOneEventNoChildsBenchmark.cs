namespace BullOak.Test.Benchmark
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;

    [ShortRunJob]
    public class LoadAggregateOneEventNoChildsBenchmark
    {
        private AggregateFixture fixture = new AggregateFixture("testing");

        public LoadAggregateOneEventNoChildsBenchmark()
        {
            fixture.AddCreationEvent();
        }

        [Benchmark]
        public object LoadRepoBaseAggregate()
        {
            using (var session = fixture.CinemaFunctionalRepo.Load(fixture.cinemaId))
            {
                return session.State;
            }
        }

        [Benchmark]
        public object LoadAggregateBasedAggregate()
            => fixture.CinemaAggregateRepository.Load(fixture.cinemaId).Result;
    }
}