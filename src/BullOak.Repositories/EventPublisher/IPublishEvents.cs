namespace BullOak.Repositories.EventPublisher
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPublishEvents
    {
        Task Publish(object @event, CancellationToken cancellationToken);
        void PublishSync(object @event);
    }
}