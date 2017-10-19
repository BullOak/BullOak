namespace BullOak.Test.Benchmark
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate;

    [ShortRunJob]
    public class RepoBasedWithVariableReconstitutors
    {
        private AggregateFixture fixture;

        [Params(5,25)]
        public int NumberOfAppliers { get; set; }

        [Params(true, false)]
        public bool Start { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var di = AggregateFixture.GetAppliersWith(NumberOfAppliers, new CinemaCreatedReconstitutor(), Start ? 0 : NumberOfAppliers - 1);
            fixture = new AggregateFixture(Guid.NewGuid().ToString(), di);
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
    }
}
