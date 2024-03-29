﻿namespace BullOak.Repositories.Upconverting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class EventUpconverter : IUpconvertStoredItems
    {
        private readonly IReadOnlyDictionary<Type, Func<ItemWithType, UpconvertResult>> upconverters;

        internal EventUpconverter(Dictionary<Type, Func<ItemWithType, UpconvertResult>> upconverters)
            => this.upconverters = upconverters ?? throw new ArgumentNullException(nameof(upconverters));

        public IEnumerable<ItemWithType> Upconvert(ItemWithType[] eventsToUpconvert)
            => Upconvert((IEnumerable<ItemWithType>) eventsToUpconvert);

        public IEnumerable<ItemWithType> Upconvert(ItemWithType eventToUpconvert)
        {
            return UpconvertWithRecursion(eventToUpconvert);
        }

        public IEnumerable<ItemWithType> Upconvert(IEnumerable<ItemWithType> eventsToUpconvert)
        {
            if (eventsToUpconvert == null) throw new ArgumentNullException(nameof(eventsToUpconvert));

            return eventsToUpconvert.SelectMany(x=> UpconvertWithRecursion(x));
        }

        public async IAsyncEnumerable<ItemWithType> Upconvert(IAsyncEnumerable<ItemWithType> eventsToUpconvert)
        {
            if (eventsToUpconvert == null) throw new ArgumentNullException(nameof(eventsToUpconvert));

            await foreach (var @event in eventsToUpconvert)
            {
                foreach(var e in UpconvertWithRecursion(@event))
                    yield return e;
            }
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

                if (result.isSingleItem) UpconvertWithRecursion(result.single, collection);
                else
                {
                    foreach (var upconverted in result.multiple)
                        UpconvertWithRecursion(upconverted, collection);
                }
            }
        }

        private IEnumerable<ItemWithType> UpconvertWithRecursion(ItemWithType item)
        {
            if (!upconverters.TryGetValue(item.type, out var upconverter))
                yield return item;
            else
            {
                var result = upconverter(item);

                if (result.isSingleItem)
                {
                    foreach(var e in UpconvertWithRecursion(result.single))
                        yield return e;
                }
                else
                {
                    foreach (var upconverted in result.multiple)
                        foreach (var e in UpconvertWithRecursion(upconverted))
                            yield return e;
                }
            }
        }

        public static explicit operator EventUpconverter(Dictionary<Type, Func<ItemWithType, UpconvertResult>> upconverters)
            => new EventUpconverter(upconverters);
    }
}
