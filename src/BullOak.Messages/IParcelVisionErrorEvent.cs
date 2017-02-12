namespace BullOak.Messages
{
    public interface IParcelVisionErrorEvent : IParcelVisionEvent
    {
        string Message { get; }
    }
}
