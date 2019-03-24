namespace BullOak.Repositories.Upconverting
{
    using System.Collections.Generic;

    internal class NullUpconverter : IUpconvertStoredItems
    {
        public IEnumerable<ItemWithType> Upconvert(ItemWithType[] eventsToUpconvert)
            => eventsToUpconvert;

        public IEnumerable<ItemWithType> Upconvert(IEnumerable<ItemWithType> eventsToUpconvert)
            => eventsToUpconvert;
    }
}
