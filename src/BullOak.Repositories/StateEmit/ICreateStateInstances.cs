namespace BullOak.Repositories.StateEmit
{
    using System;

    public interface ICreateStateInstances
    {
        object GetState(Type type);
    }
}