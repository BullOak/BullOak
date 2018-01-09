namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;
    using BullOak.Repositories.Appliers;
    using BullOak.Repositories.StateEmit;
    using BullOak.Repositories.StateEmit.Emitters;
    using BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate;

    public class TestBlankApplier<TEvent> : BaseApplyEvents<CinemaAggregateState, TEvent>
    {
        public override CinemaAggregateState Apply(CinemaAggregateState state, TEvent @event) => state;
    }

    public interface IFlagForApplier
    {
        int MyValue { get; set; }
    }

    [ShortRunJob]
    public class RepoBasedWithVariableReconstitutors
    {
        private AggregateFixture fixture;

        [Params(5, 25, 40)]
        public int Appliers { get; set; }

        [Params(1, 10)]
        public int Events { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var flagType = typeof(IFlagForApplier);
            var openApplierType = typeof(TestBlankApplier<>);

            var appliers = new List<object>();
            for (int i = 0; i < Appliers; i++)
            {
                var stubClass = StateTypeEmitter.EmitType(flagType, new OwnedStateClassEmitter(), flagType.Name + i.ToString());
                var constructedApplierType = openApplierType.MakeGenericType(stubClass);
                appliers.Add(Activator.CreateInstance(constructedApplierType));
            }

            fixture = new AggregateFixture(Guid.NewGuid().ToString(), appliers.ToArray());

            for(int i =0;i<Events;i++)
            {
                fixture.AddCinemaCreationEvent();
            }
        }

        [Benchmark]
        public object LoadRepoBaseAggregate()
        {
            using (var session = fixture.CinemaFunctionalRepo.BeginSessionFor(fixture.cinemaId))
            {
                return session.GetCurrentState();
            }
        }
    }
}
