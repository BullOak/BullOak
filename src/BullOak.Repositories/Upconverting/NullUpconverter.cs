namespace BullOak.Repositories.Upconverting
{
    using System.Collections.Generic;

    internal class NullUpconverter : IUpconvertStoredItems
    {
        public IEnumerable<ItemWithType> Upconvert(ItemWithType[] eventsToUpconvert)
            => eventsToUpconvert;
    }
}
