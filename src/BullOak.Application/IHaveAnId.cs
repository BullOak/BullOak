namespace BullOak.Application
{
    using BullOak.Common;

    public interface IHaveAnId<out T>
        where T : IId
    {
        T Id { get; }
    }
}