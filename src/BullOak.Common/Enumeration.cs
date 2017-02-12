namespace BullOak.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    [Serializable]
    [DebuggerDisplay("{DisplayName} - {Value}")]
    public abstract class Enumeration<TEnumeration, TValue> : IComparable<TEnumeration>, IEquatable<TEnumeration>
        where TEnumeration : Enumeration<TEnumeration, TValue>
        where TValue : IComparable
    {
        private static readonly Lazy<TEnumeration[]> Enumerations = new Lazy<TEnumeration[]>(GetEnumerations);

        private readonly string displayName;
        private readonly TValue value;



        protected Enumeration(TValue value, string displayName)
        {
            this.value = value;
            this.displayName = displayName;
        }

        public TValue Value
        {
            get { return value; }
        }

        public string DisplayName
        {
            get { return displayName; }
        }

        public static TEnumeration[] GetAll()
        {
            return Enumerations.Value;
        }

        public static TEnumeration FromValue(TValue value)
        {
            return Parse(value, "value", item => item.Value.Equals(value));
        }

        public static TEnumeration Parse(string displayName)
        {
            return Parse(displayName, "display name", item => item.DisplayName == displayName);
        }

        public static bool TryParse(TValue value, out TEnumeration result)
        {
            return TryParse(x => x.Value.Equals(value), out result);
        }

        public static bool TryParse(string displayName, out TEnumeration result)
        {
            return TryParse(x => x.DisplayName == displayName, out result);
        }

        public int CompareTo(TEnumeration other)
        {
            return Value.CompareTo(other.Value);
        }

        public override sealed string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TEnumeration);
        }

        public bool Equals(TEnumeration other)
        {
            return other != null && Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        private static bool TryParse(Func<TEnumeration, bool> predicate, out TEnumeration result)
        {
            result = GetAll().FirstOrDefault(predicate);

            return result != null;
        }

        private static TEnumeration[] GetEnumerations()
        {
            var enumerationType = typeof(TEnumeration);

            return enumerationType
           .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
           .Where(info => enumerationType.IsAssignableFrom(info.FieldType))
           .Select(info => info.GetValue(null))
           .Cast<TEnumeration>()
           .ToArray();
        }

        private static TEnumeration Parse(object value, string description, Func<TEnumeration, bool> predicate)
        {
            TEnumeration result;

            if (!TryParse(predicate, out result))
            {
                var message = string.Format("'{0}' is not a valid {1} in {2}", value, description, typeof(TEnumeration));
                throw new ArgumentException(message, "value");
            }

            return result;
        }
    }
}
