namespace BullOak.Repositories.StateEmit
{
    public interface ICanSwitchBackAndToReadOnly
    {
        bool CanEdit { set; }
    }
}
