using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataCloner.Core.Framework
{
    public static class ArrayExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> list, Predicate<T> condition)
        {
            var i = -1;
            return list.Any(x =>
            {
                i++;
                return condition(x);
            })
                ? i
                : -1;
        }
    }
}
