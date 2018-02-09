namespace BullOak.Repositories
{
    public struct Maybe<T>
    {
        private static readonly Maybe<T> none = new Maybe<T>(default(T), false);

        public readonly bool hasValue;
        public readonly T value;

        public static Maybe<T> None => none;

        private Maybe(T value, bool hasValue)
        {
            this.hasValue = hasValue;
            this.value = value;
        }

        public static Maybe<T> Some(T value)
            => new Maybe<T>(value, true);
    }
}