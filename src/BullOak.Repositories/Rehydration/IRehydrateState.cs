namespace BullOak.Repositories.Rehydration
{
    using System.Collections.Generic;

    public interface IRehydrateState
    {
        TState RehydrateFrom<TState>(IEnumerable<ItemWithType> events, TState initialState = default);
    }
}