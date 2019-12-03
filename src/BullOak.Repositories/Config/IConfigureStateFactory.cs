namespace BullOak.Repositories
{
    using BullOak.Repositories.StateEmit;

    public interface IConfigureStateFactory : IConfigureBullOak
    {
        IConfigureThreadSafety WithDefaultStateFactory();
        IConfigureThreadSafety WithStateFactory(ICreateStateInstances stateFactory);
    }
}
