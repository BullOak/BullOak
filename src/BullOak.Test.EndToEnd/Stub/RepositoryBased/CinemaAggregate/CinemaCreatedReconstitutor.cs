namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate
{
    using BullOak.Repositories;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    public class CinemaCreatedReconstitutor : BaseApplyEvents<CinemaAggregateState, CinemaCreated>
    {
        public override CinemaAggregateState Apply(CinemaAggregateState state, IHoldEventWithMetadata<CinemaCreated> @event)
        {
            state.Id = @event.Event.Id;
            state.NumberOfSeats = @event.Event.Capacity;

            return state;
        }
    }
}
