using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DataCloner.Core.Framework
{
    public static class Extensions
    {
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (index < 0 || index >= source.Length)
                throw new ArgumentOutOfRangeException("index", index, "index is outside the bounds of source array");

            var destination = new T[source.Length - 1];
            Array.Copy(source, 0, destination, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, destination, index, source.Length - index - 1);

            return destination;
        }

        public static T[] Remove<T>(this T[] source, T obj)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var idx = Array.IndexOf(source, obj);
            if (idx != -1)
                return source.RemoveAt(idx);
            return source;
        }

        public static T[] Add<T>(this T[] source, T obj)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var arrCopy = source;
            var size = arrCopy.Length;
            Array.Resize(ref arrCopy, size + 1);
            arrCopy[size] = obj;
            return arrCopy;
        }

        public static string SerializeXml<T>(this T obj)
        {
            var xs = new DataContractSerializer(obj.GetType());
            using (var ms = new MemoryStream())
            using (var sr = new StreamReader(ms))
            {
                xs.WriteObject(ms, obj);
                ms.Position = 0;
                return sr.ReadToEnd();
            }
        }

        public static T DeserializeXml<T>(this string str)
        {
            var xs = new DataContractSerializer(typeof(T));
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                sw.Write(str);
                ms.Position = 0;
                return (T)xs.ReadObject(ms);
            }
        }

        public static void SaveXml<T>(this T obj, string path)
        {
            var xs = new DataContractSerializer(obj.GetType());
            using (var fs = new FileStream(path, FileMode.Create))
            {
                xs.WriteObject(fs, obj);
            }
        }

        public static T LoadXml<T>(string path)
        {
            var xs = new DataContractSerializer(typeof (T));
            if (!File.Exists(path)) return default(T);
            using (var fs = new FileStream(path, FileMode.Open))
            {
                return (T)xs.ReadObject(fs);
            };
        }

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

        public static int? ToNullableInt32(this string input)
        {
            int i;
            return Int32.TryParse(input, out i) ? (int?)i : null;
        }      

        /// <summary>
        /// Build a SQL text query from a DbCommand.
        /// </summary>
        /// <param name="dbCommand">The query</param>
        /// <returns>SQL query</returns>
        public static string GetGeneratedQuery(this IDbCommand dbCommand)
        {
            var query = dbCommand.CommandText;
            foreach (var parameter in dbCommand.Parameters)
            {
                var param = parameter as IDataParameter;
                string newValue;

                if (param.Direction == ParameterDirection.Output)
                    newValue = param.ParameterName + "/*" + param.Value.ToString().EscapeSql() + "*/";
                else
                    newValue = param.Value.ToString().EscapeSql();

                query = query.Replace(param.ParameterName + " ", newValue + " ");
                query = query.Replace(param.ParameterName + ",", newValue + ",");
                query = query.Replace(param.ParameterName + ")", newValue + ")");
            }
            return query;
        }

        internal static string FormatSqlParam(this string value)
        {
            return value.Replace(" ", String.Empty);
        }

        internal static string EscapeSql(this string value)
        {
            return value.Replace("'", "''");
        }
    }
}
