namespace BullOak.Repositories.StateEmit
{
    public interface IControlStateWritability<TState>
    {
        void MakeStateWritable();
        void MakeStateReadOnly();
        TState State { get; }
    }
}
