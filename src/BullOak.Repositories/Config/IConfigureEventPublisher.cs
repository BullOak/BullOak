namespace BullOak.Repositories
{
    using BullOak.Repositories.EventPublisher;

    public interface IConfigureEventPublisher : IConfigureBullOak
    {
        IManuallyConfigureEventAppliers WithEventPublisher(IPublishEvents EventPublisher);
    }
}
