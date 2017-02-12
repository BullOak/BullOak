namespace BullOak.Application.MethodBuilderContainer
{
    using System;
    using System.Collections.Generic;

    internal class TypeArrayEqualityComparer : IEqualityComparer<Type[]>
    {
        public bool Equals(Type[] x, Type[] y)
        {
            if (BothNull(x, y)) return true;
            if (AnyNull(x, y)) return false;
            if (AreDifferentSize(x, y)) return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) return false;
            }

            return true;
        }

        private bool AreDifferentSize(Type[] x, Type[] y) => x.Length != y.Length;
        private bool AnyNull(Type[] x, Type[] y) => x == null || y == null;
        private bool BothNull(Type[] x, Type[] y) => x == null && y == null;

        public int GetHashCode(Type[] types)
        {
            int result = 37;

            foreach (var type in types)
            {
                result *= 397;
                result += type.GetHashCode();
            }

            return result;
        }
    }
}