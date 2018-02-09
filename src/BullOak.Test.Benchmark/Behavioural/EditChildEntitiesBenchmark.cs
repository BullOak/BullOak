namespace BullOak.Test.Benchmark.Behavioural
{
    using System;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Jobs;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;

    using AggregateBasedViewing = BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate.ViewingAggregateRoot;
    using RepoBasedViewing = BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate.ViewingAggregateRoot;

    [ShortRunJob]
    public class EditChildEntitiesBenchmark
    {
        private AggregateFixture fixture;
        private static readonly RepoBasedViewing viewingAggregate = new RepoBasedViewing();

        private ViewingId viewingId;

        [Params(20, 100)]
        public int Capacity { get; set; }

        [Params(1, 20)]
        public int SeatsToReserve { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            fixture = new AggregateFixture(Guid.NewGuid().ToString());
            viewingId = new ViewingId(Guid.NewGuid().ToString(), fixture.dateOfViewing, fixture.cinemaId);
            fixture.AddViewingAndSeatCreationEvents(viewingId, Capacity);
        }

        [Benchmark]
        public void EditChildFromRepoBasedAggregate()
        {
            using (var session = fixture.ViewingFunctionalRepo.BeginSessionFor(viewingId).Result)
            {
                for (int i = 0; i < SeatsToReserve; i++)
                {
                    var events = viewingAggregate.ReserveSeat(session.GetCurrentState(), i);

                    session.AddEvent(events);
                }
                session.SaveChanges().Wait();
            }

            var eventCount = fixture.ViewingFunctionalRepo[viewingId].Length;
            var buffer = fixture.ViewingFunctionalRepo[viewingId];
            Array.Resize(ref buffer, eventCount - SeatsToReserve);
            fixture.ViewingFunctionalRepo[viewingId] = buffer;
        }

        //[Benchmark]
        public void EditChildFromAggregateBasedAggregate()
        {
            var aggregate = fixture.ViewingAggregateRepository.Load(viewingId).Result;

            for (int i = 0; i < SeatsToReserve; i++)
                aggregate.ReserveSeat((ushort) i);

            fixture.ViewingAggregateRepository.Save(aggregate).Wait();

            var eventCount = fixture.ViewingAggregateRepository[viewingId.ToString()].Count;
            fixture.ViewingAggregateRepository[viewingId.ToString()].RemoveRange(eventCount - SeatsToReserve, SeatsToReserve);
        }
    }
}
