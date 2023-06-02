namespace BullOak.Repositories.Upconverting
{
    using System.Collections.Generic;

    public interface IUpconvertStoredItems
    {
        IAsyncEnumerable<ItemWithType> Upconvert(IAsyncEnumerable<ItemWithType> eventsToUpconvert);
        IEnumerable<ItemWithType> Upconvert(IEnumerable<ItemWithType> eventsToUpconvert);
        IEnumerable<ItemWithType> Upconvert(ItemWithType[] eventsToUpconvert);
        IEnumerable<ItemWithType> Upconvert(ItemWithType eventToUpconvert);
    }
}
