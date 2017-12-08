namespace BullOak.Repositories.StateEmit
{
    internal interface ICanSwitchBackAndToReadOnly
    {
        bool CanEdit { set; }
    }
}