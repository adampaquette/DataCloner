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

        public static void CopyTo(this Stream source, Stream destination, int bufferSize, int count)
        {
            var buffer = new byte[bufferSize];
            int read;

            while ((read = source.Read(buffer, 0, Math.Min(buffer.Length, count))) > 0)
            {
                destination.Write(buffer, 0, read);
                count -= read;
            }
        }
    }
}
