namespace BullOak.Test.Benchmark.Behavioural
{
    using BenchmarkDotNet.Attributes;

    [ShortRunJob]
    public class LoadAggregateOneEventNoChildsBenchmark
    {
        private AggregateFixture fixture = new AggregateFixture("testing");

        public LoadAggregateOneEventNoChildsBenchmark()
        {
            fixture.AddCinemaCreationEvent();
        }

        [Benchmark]
        public object LoadRepoBaseAggregate()
        {
            using (var session = fixture.CinemaFunctionalRepo.BeginSessionFor(fixture.cinemaId).Result)
            {
                return session.GetCurrentState();
            }
        }
    }
}
