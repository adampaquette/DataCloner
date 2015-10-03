using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DataCloner.Framework
{
    public static class Extensions
    {
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            if (source == null)
                throw new ArgumentNullException("source");

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
                throw new ArgumentNullException("source");

            if (obj == null)
                throw new ArgumentNullException("obj");

            var idx = Array.IndexOf(source, obj);
            if (idx != -1)
                return source.RemoveAt(idx);
            return source;
        }

        public static T[] Add<T>(this T[] source, T obj)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (obj == null)
                throw new ArgumentNullException("obj");

            var arrCopy = source;
            var size = arrCopy.Length;
            Array.Resize(ref arrCopy, size + 1);
            arrCopy[size] = obj;
            return arrCopy;
        }

        public static string SerializeXml<T>(this T obj)
        {
            var xs = new XmlSerializer(obj.GetType());
            var sw = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(sw, obj, ns);
            return sw.ToString();
        }

        public static T DeserializeXml<T>(this string str)
        {
            var xs = new XmlSerializer(typeof (T));
            var sr = new StringReader(str);
            return (T) xs.Deserialize(sr);
        }

        public static void SaveXml<T>(this T obj, string path)
        {
            var xs = new XmlSerializer(obj.GetType());
            var fs = new FileStream(path, FileMode.Create);
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(fs, obj, ns);
            fs.Close();
        }

        public static T LoadXml<T>(string path)
        {
            var xs = new XmlSerializer(typeof (T));
            if (!File.Exists(path)) return default(T);
            var fs = new FileStream(path, FileMode.Open);
            var cReturn = (T) xs.Deserialize(fs);
            fs.Close();
            return cReturn;
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

        public static bool IsVariable(this string value)
        {
            if (value == null) return false;
            return value.StartsWith("{$") && value.EndsWith("}");
        }

        public static string ExtractVariableKey(this string value)
        {
            int len;
            var posStart = value.IndexOf('{');
            if (posStart != 0) return null;

            posStart = value.IndexOf('{', 1);
            if (posStart == -1)
                len = value.Length - 1;
            else
                len = posStart;

            return value.Substring(0, len) + "}";
        }

        public static string ExtractVariableValue(this string value)
        {
            var posStart = value.IndexOf('{');
            if (posStart != 0) return null;

            posStart = value.IndexOf('{', 1);
            if (posStart == -1) return null;

            var posEnd = value.IndexOf('}', posStart);
            if (posEnd == -1) return null;

            return value.Substring(posStart, posEnd - posStart);
        }

        public static Int16 ExtractVariableValueInt16(this string value)
        {
            var extractedValue = value.ExtractVariableValue();
            Int16 result = 0;
            Int16.TryParse(extractedValue, out result);
            return result;
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
