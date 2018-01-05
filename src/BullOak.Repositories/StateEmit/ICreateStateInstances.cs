namespace BullOak.Repositories.StateEmit
{
    using System;
    using System.Collections.Generic;

    public interface ICreateStateInstances
    {
        void WarmupWith(IEnumerable<Type> typesToCreateFactoriesFor);
        object GetState(Type type);
    }
}