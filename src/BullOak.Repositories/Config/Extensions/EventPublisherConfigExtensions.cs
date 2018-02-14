namespace BullOak.Repositories
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BullOak.Repositories.EventPublisher;

    public static class EventPublisherConfigExtensions
    {
        public static IManuallyConfigureEventAppliers WithEventPublisher(
            this IConfigureEventPublisher eventPublisherConfig,
            Action<object> publish)
            => eventPublisherConfig.WithEventPublisher(new MySyncEventPublisher(publish));

        public static IManuallyConfigureEventAppliers WithEventPublisher(
            this IConfigureEventPublisher eventPublisherConfig,
            Func<object, Task> publish)
            => eventPublisherConfig.WithEventPublisher(new MyAsyncEventPublisher((o, c) => publish(o)));

        public static IManuallyConfigureEventAppliers WithEventPublisher(
            this IConfigureEventPublisher eventPublisherConfig,
            Func<object, CancellationToken, Task> publish)
            => eventPublisherConfig.WithEventPublisher(new MyAsyncEventPublisher(publish));

        public static IManuallyConfigureEventAppliers WithNoEventPublisher(
            this IConfigureEventPublisher eventPublisherConfig)
            => eventPublisherConfig.WithEventPublisher(MyNullEventPublisher.Instance);
    }
}
