namespace BullOak.Repositories.Upconverting
{
    using System;
    using System.Collections.Generic;

    internal struct UpconvertResult
    {
        public readonly bool isSingleItem;
        public readonly ItemWithType single;
        public readonly IEnumerable<ItemWithType> multiple;

        public UpconvertResult(ItemWithType single)
        {
            isSingleItem = true;
            this.single = single;
            this.multiple = default(IEnumerable<ItemWithType>);
        }

        public UpconvertResult(IEnumerable<ItemWithType> multiple)
        {
            isSingleItem = false;
            this.multiple = multiple;
            this.single = default(ItemWithType);
        }

        public static UpconvertResult GetFrom(object result)
        {
            if(result is IEnumerable<ItemWithType> collection)
                return new UpconvertResult(collection);
            if (result is ItemWithType item)
                return new UpconvertResult(item);

            throw new ArgumentException(
                $"Argument is not of correct type. Expected either {typeof(ItemWithType).Name} or {typeof(IEnumerable<ItemWithType>).Name}",
                nameof(result));
        }
    }
}