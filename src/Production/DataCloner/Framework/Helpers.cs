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

    internal static class GeneralExtensionHelper
    {
        /// <summary>
        /// Impersonnification du schéma
        /// </summary>
        /// <param name="serverId"></param>
        internal static Int16 Impersonate(Int16 serverId)
        {
            Int16 id = QueryDispatcher.Cache.ConnectionStrings.Where(c => c.Id == serverId).First().SameConfigAsId;
            if (id > 0)
                return id;
            return serverId;
        }
    }
}
