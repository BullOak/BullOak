namespace BullOak.Test.EndToEnd.Stub.AggregateBased.ViewingAggregate
{
    using BullOak.Application;
    using BullOak.Test.EndToEnd.Stub.Shared.Ids;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    public class SeatsInViewing : ChildEntity<SeatId, ViewingAggregateRoot>,
        IPublish<SeatInViewingInitialized>,
        IPublish<SeatReservedEvent>
    {
        public bool Reserved { get; private set; }

        //Needed by framework
        public SeatsInViewing()
        { }

        public SeatsInViewing(int id)
        {
            ApplyEvent(new SeatInViewingInitialized(new SeatId((ushort)id)));
        }

        public void Reserve()
        {
            ApplyEvent(new SeatReservedEvent(this.Parent.Id, Id));
        }

        public void Apply(SeatInViewingInitialized @event)
        {
            Id = @event.SeatId;
            Reserved = false;
        }

        public void Apply(SeatReservedEvent @event)
        {
            Reserved = true;
        }
    }
}
