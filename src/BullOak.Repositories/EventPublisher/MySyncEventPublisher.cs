namespace BullOak.Repositories.EventPublisher
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class MySyncEventPublisher : IPublishEvents
    {
        private static readonly Task Done = Task.FromResult(0);
        public readonly Action<ItemWithType> publish;

        public MySyncEventPublisher(Action<ItemWithType> publish)
            => this.publish = publish ?? throw new ArgumentNullException(nameof(publish));

        public Task Publish(ItemWithType @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            PublishSync(@event);
            return Done;
        }

        public void PublishSync(ItemWithType @event) => publish(@event);
    }
}
