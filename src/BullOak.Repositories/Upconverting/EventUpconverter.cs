namespace BullOak.Repositories.Upconverting
{
    using System;
    using System.Collections.Generic;

    internal class EventUpconverter : IUpconvertStoredItems
    {
        private readonly IReadOnlyDictionary<Type, Func<ItemWithType, UpconvertResult>> upconverters;

        internal EventUpconverter(Dictionary<Type, Func<ItemWithType, UpconvertResult>> upconverters)
            => this.upconverters = upconverters ?? throw new ArgumentNullException(nameof(upconverters));

        public IEnumerable<ItemWithType> Upconvert(ItemWithType[] eventsToUpconvert)
            => Upconvert((IEnumerable<ItemWithType>) eventsToUpconvert);

        public IEnumerable<ItemWithType> Upconvert(IEnumerable<ItemWithType> eventsToUpconvert)
        {
            if (eventsToUpconvert == null) throw new ArgumentNullException(nameof(eventsToUpconvert));

            var upconverted = new List<ItemWithType>();

            foreach(var @event in eventsToUpconvert)
            {
                UpconvertWithRecursion(@event, upconverted);
            }

            return upconverted;
        }

        /// <summary>
        /// Recursively upconverts each item. This uses recursion but also a provided List so as to avoid situation where we either create
        ///  enums or collections for each step in the recursion. Some of the problem here is because one event can upconvert to multiple.
        /// </summary>
        /// <param name="item">The item to attempt to upconvert.</param>
        /// <param name="collection">The collection to store the upconverted events into.</param>
        private void UpconvertWithRecursion(ItemWithType item, List<ItemWithType> collection)
        {
            if (!upconverters.TryGetValue(item.type, out var upconverter))
            {
                collection.Add(item);
            }
            else
            {
                var result = upconverter(item);

                if(result.isSingleItem) UpconvertWithRecursion(result.single, collection);
                else
                {
                    foreach(var upconverted in result.multiple)
                        UpconvertWithRecursion(upconverted, collection);
                }
            }
        }

        public static explicit operator EventUpconverter(Dictionary<Type, Func<ItemWithType, UpconvertResult>> upconverters)
            => new EventUpconverter(upconverters);
    }
}