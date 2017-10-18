namespace BullOak.Test.EndToEnd.Stub.RepositoryBased.ViewingAggregate
{
    using System;
    using BullOak.Repositories;
    using BullOak.Test.EndToEnd.Stub.Shared.Messages;

    internal class ViewingReconstitutor : IApplyEvents<ViewingState>
    {
        public bool CanApplyEvent(object @event)
            => @event is ViewingCreatedEvent || @event is SeatReservedEvent;

        public ViewingState Apply(ViewingState state, IHoldEventWithMetadata envelope)
        {
            switch (envelope)
            {
                case IHoldEventWithMetadata<ViewingCreatedEvent> env:
                    return Apply(state,env);
                case IHoldEventWithMetadata<SeatReservedEvent> env:
                    return Apply(state, env);
                    default:
                throw new Exception("Cannot apply event");
            }
        }

        public ViewingState Apply(ViewingState state, IHoldEventWithMetadata<ViewingCreatedEvent> envelope)
        {
            //Use Automapper here??
            state.ViewingId = envelope.Event.Id;
            state.Seats = new Seats[envelope.Event.Seats];
            for (int i = 0; i < state.Seats.Length; i++)
                state.Seats[i] = new Seats {Id = i, IsReserved = false};

            return state;
        }

        public ViewingState Apply(ViewingState state, IHoldEventWithMetadata<SeatReservedEvent> envelope)
        {
            state.Seats[envelope.Event.IdOfSeatToReserve.Id].IsReserved = true;
            return state;
        }
    }
}
