namespace BullOak.EventStream
{
    using BullOak.Messages;
    using System.Threading.Tasks;

    public interface IEventPublisher
    {
        Task Publish<T>(T @event) where T : class, IParcelVisionEvent;
    }
}
