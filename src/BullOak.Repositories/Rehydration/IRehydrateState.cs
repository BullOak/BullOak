namespace BullOak.Repositories.Rehydration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRehydrateState
    {
        TState RehydrateFrom<TState>(IEnumerable<ItemWithType> events, TState initialState = default);
        Task<TState> RehydrateFrom<TState>(IAsyncEnumerable<ItemWithType> events, TState initialState = default);
    }
}
