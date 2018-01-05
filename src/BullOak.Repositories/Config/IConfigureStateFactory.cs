namespace BullOak.Repositories
{
    using BullOak.Repositories.StateEmit;

    public interface IConfigureStateFactory
    {
        IConfigureThreadSafety WithDefaultStateFactory();
        IConfigureThreadSafety WithStateFactory(ICreateStateInstances stateFactory);
    }
}