namespace BullOak.Repositories.EventPublisher
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class MyAsyncEventPublisher : IPublishEvents
    {
        public readonly Func<ItemWithType, CancellationToken, Task> publish;

        public MyAsyncEventPublisher(Func<ItemWithType, CancellationToken, Task> publish)
            => this.publish = publish ?? throw new ArgumentNullException(nameof(publish));

        public Task Publish(ItemWithType @event, CancellationToken cancellationToken = default(CancellationToken)) => publish(@event, cancellationToken);

        public void PublishSync(ItemWithType @event)
            => Task.Run(async () => await publish(@event, default(CancellationToken)).ConfigureAwait(false)).Wait();
    }
}
