namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.CinemaAggregate
{
    using BullOak.Repositories.EventSourced;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    public class CinemaCreatedReconstitutor : BaseHandler<CinemaAggregateState, CinemaCreated>
    {
        protected override CinemaAggregateState Apply(CinemaAggregateState state, CinemaCreated @event)
        {
            state.Id = @event.Id;
            state.NumberOfSeats = @event.Capacity;

            return state;
        }
    }
}
