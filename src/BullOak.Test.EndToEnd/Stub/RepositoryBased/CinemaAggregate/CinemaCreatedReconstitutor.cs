namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate
{
    using BullOak.Repositories.Appliers;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    public class CinemaCreatedReconstitutor : BaseApplyEvents<CinemaAggregateState, CinemaCreated>
    {
        public override CinemaAggregateState Apply(CinemaAggregateState state, CinemaCreated @event)
        {
            state.Id = @event.Id;
            state.NumberOfSeats = @event.Capacity;

            return state;
        }
    }
}
