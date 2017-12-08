namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate
{
    using System;
    using BullOak.Repositories;
    using BullOak.Repositories.Appliers;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    internal class ViewingReconstitutor : IApplyEvents<ViewingState>
    {
        public bool CanApplyEvent(object @event)
            => @event is ViewingCreatedEvent || @event is SeatReservedEvent || @event is SeatInViewingInitialized;

        public ViewingState Apply(ViewingState state, object envelope)
        {
            switch (envelope)
            {
                case ViewingCreatedEvent env:
                    return Apply(state,env);
                case SeatReservedEvent env:
                    return Apply(state, env);
                case SeatInViewingInitialized env:
                    return Apply(state, env);
                    default:
                throw new Exception("Cannot apply event");
            }
        }

        public ViewingState Apply(ViewingState state, ViewingCreatedEvent envelope)
        {
            //Use Automapper here??
            state.ViewingId = envelope.Id;
            state.Seats = new Seats[envelope.Seats];
            for (int i = 0; i < state.Seats.Length; i++)
                state.Seats[i] = new Seats {Id = i, IsReserved = false};

            return state;
        }

        public ViewingState Apply(ViewingState state, SeatReservedEvent envelope)
        {
            state.Seats[envelope.IdOfSeatToReserve.Id].IsReserved = true;
            return state;
        }

        public ViewingState Apply(ViewingState state, SeatInViewingInitialized @event)
            => state;
    }
}
