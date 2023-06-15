namespace BullOak.Repositories.Rehydration
{
    using BullOak.Repositories.Appliers;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public struct RehydrateFromResult<TState>
    {
        public readonly bool IsStateDefault;
        public readonly TState State;
        public readonly long? LastEventIndex;

        public RehydrateFromResult(TState state, bool isStateDefault, long? lastEventIndex)
        {
            State = state;
            IsStateDefault = isStateDefault;
            LastEventIndex = lastEventIndex;
        }
    }

    public interface IRehydrateState
    {
        RehydrateFromResult<TState> RehydrateFrom<TState>(IEnumerable<StoredEvent> events, TState initialState = default);
        Task<RehydrateFromResult<TState>> RehydrateFrom<TState>(IAsyncEnumerable<StoredEvent> events, TState initialState = default);
    }
}
