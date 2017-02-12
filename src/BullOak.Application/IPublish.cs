namespace BullOak.Application
{
    using BullOak.Messages;

    public interface IPublish<in T> where T : IParcelVisionEvent
    {
        void Apply(T @event);
    }
}
