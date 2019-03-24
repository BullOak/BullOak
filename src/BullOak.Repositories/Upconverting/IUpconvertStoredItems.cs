namespace BullOak.Repositories.Upconverting
{
    using System.Collections.Generic;

    public interface IUpconvertStoredItems
    {
        IEnumerable<ItemWithType> Upconvert(IEnumerable<ItemWithType> eventsToUpconvert);
        IEnumerable<ItemWithType> Upconvert(ItemWithType[] eventsToUpconvert);
    }
}