namespace BullOak.Repositories.Rehydration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public struct RehydrateFromResult<TState>
    {
        public readonly bool IsStateDefault;
        public readonly TState State;

        public RehydrateFromResult(TState state, bool isStateDefault)
        {
            State = state;
            IsStateDefault = isStateDefault;
        }
    }

    public interface IRehydrateState
    {
        RehydrateFromResult<TState> RehydrateFrom<TState>(IEnumerable<ItemWithType> events, TState initialState = default);
        Task<RehydrateFromResult<TState>> RehydrateFrom<TState>(IAsyncEnumerable<ItemWithType> events, TState initialState = default);
    }
}
