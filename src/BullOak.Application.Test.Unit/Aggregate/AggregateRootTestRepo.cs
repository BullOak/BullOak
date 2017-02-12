namespace BullOak.Application.Test.Unit.Aggregate
{
    using BullOak.Application;

    internal class AggregateRootTestRepo : AggregateRepositoryBase<AggregateRootTest, AggregateRootTestId>
    {
        public AggregateRootTestRepo(BullOak.EventStream.IEventStore eventStore)
            : base(eventStore)
        { }
    }
}
