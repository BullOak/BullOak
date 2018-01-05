namespace BullOak.Repositories
{
    using System;

    public interface IReadDefaultConfiguration
    {
        Type TypeOfEventCollection { get; }
    }
}