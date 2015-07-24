using System.Collections;
using System.Collections.Generic;

namespace DataCloner.Framework
{
    public class StructuralEqualityComparer<T> : IEqualityComparer<T>
    {
        private static StructuralEqualityComparer<T> _defaultComparer;

        public bool Equals(T x, T y)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
        }

        public static StructuralEqualityComparer<T> Default
        {
            get { return _defaultComparer ?? (_defaultComparer = new StructuralEqualityComparer<T>()); }
        }
    }
}
