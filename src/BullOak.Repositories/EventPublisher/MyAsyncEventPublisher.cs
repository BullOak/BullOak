namespace BullOak.Repositories.EventPublisher
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class MyAsyncEventPublisher : IPublishEvents
    {
        public readonly Func<object, CancellationToken?, Task> publish;

        public MyAsyncEventPublisher(Func<object, CancellationToken?, Task> publish)
            => this.publish = publish ?? throw new ArgumentNullException(nameof(publish));

        public Task Publish(object @event, CancellationToken? cancellationToken = null) => publish(@event, cancellationToken);

        public void PublishSync(object @event)
            => Task.Run(async () => await publish(@event, null).ConfigureAwait(false)).Wait();
    }
}