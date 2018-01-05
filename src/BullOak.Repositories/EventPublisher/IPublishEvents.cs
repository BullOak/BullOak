namespace BullOak.Repositories.EventPublisher
{
    using System.Threading.Tasks;

    public interface IPublishEvents
    {
        Task Publish(object @event);
        void PublishSync(object @event);
    }
}