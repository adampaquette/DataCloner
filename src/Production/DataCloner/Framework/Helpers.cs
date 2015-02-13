using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DataCloner.DataAccess;

namespace DataCloner.Framework
{
    public class StructuralEqualityComparer<T> : IEqualityComparer<T>
    {
        private static StructuralEqualityComparer<T> defaultComparer;

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
            get
            {
                if (defaultComparer == null)
                {
                    defaultComparer = new StructuralEqualityComparer<T>();
                }
                return defaultComparer;
            }
        }
    }
}
