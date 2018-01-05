namespace BullOak.Repositories.Session
{
    /// <summary>
    /// Manages a session and holds a sync construct. A session represents the operations that relate
    /// to loading from db, mutating the state (in BullOak this happens exclusively through emitting
    /// events that may or may not be the data stored depending if you chose an ES repository) and
    /// then saving it. The sync construct is the construct that is used for optimistic concurrency
    /// (named like this to support different storage mechanisms that use versions, dates, etags etc.)
    /// </summary>
    /// <typeparam name="TState">The type of the state that this session manages</typeparam>
    /// <typeparam name="TSyncConstruct">The type of the sync construct.</typeparam>
    public interface IManageSessionAndHoldSyncConstructOf<out TState, out TSyncConstruct> : IManageSessionOf<TState>
    {
        TSyncConstruct SyncConstruct { get; }
    }
}