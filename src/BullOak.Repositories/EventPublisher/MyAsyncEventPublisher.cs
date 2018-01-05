namespace BullOak.Repositories.EventPublisher
{
    using System;
    using System.Threading.Tasks;

    internal class MyAsyncEventPublisher : IPublishEvents
    {
        public readonly Func<object, Task> publish;

        public MyAsyncEventPublisher(Func<object, Task> publish)
            => this.publish = publish ?? throw new ArgumentNullException(nameof(publish));

        public Task Publish(object @event) => publish(@event);

        public void PublishSync(object @event)
            => Task.Run(async () => await publish(@event).ConfigureAwait(false)).Wait();
    }
}