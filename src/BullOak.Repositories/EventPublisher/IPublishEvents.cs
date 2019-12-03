namespace BullOak.Repositories.EventPublisher
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPublishEvents
    {
        Task Publish(ItemWithType @event, CancellationToken cancellationToken);
        void PublishSync(ItemWithType @event);
    }
}
