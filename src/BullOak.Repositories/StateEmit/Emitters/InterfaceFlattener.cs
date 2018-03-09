namespace BullOak.Repositories.StateEmit.Emitters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;

    internal class InterfaceFlattener
    {
        private class PropertyComparer : IEqualityComparer<Tuple<Type, PropertyInfo>>
        {
            public bool Equals(Tuple<Type, PropertyInfo> x, Tuple<Type, PropertyInfo> y)
                => x.Item2.PropertyType == y.Item2.PropertyType
                   && x.Item2.Name == y.Item2.Name;

            public int GetHashCode(Tuple<Type, PropertyInfo> obj)
                => obj.Item2.Name.GetHashCode() ^ obj.Item2.PropertyType.GetHashCode();
        }

        private static readonly PropertyComparer comparer = new PropertyComparer();

        public static IEnumerable<Tuple<Type, PropertyInfo>> Dedup(IEnumerable<Tuple<Type, PropertyInfo>> input)
            => input.Distinct(comparer);

        public static IEnumerable<Tuple<Type, PropertyInfo>> GetAllProperties(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException("Type should be an interface");

            foreach (var prop in interfaceType.GetProperties())
                yield return new Tuple<Type, PropertyInfo>(interfaceType, prop);

            foreach(var @interface in interfaceType.GetInterfaces())
            foreach (var propInfo in GetAllProperties(@interface))
                yield return propInfo;
        }
    }
}
