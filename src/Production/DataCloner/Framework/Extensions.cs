using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

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

            T[] destination = new T[source.Length - 1];
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

            int idx = Array.IndexOf(source, obj);
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

            T[] arrCopy = source;
            int size = arrCopy.Length;
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
            var xs = new XmlSerializer(typeof(T));
            var sr = new StringReader(str);
            return (T)xs.Deserialize(sr);
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
            var xs = new XmlSerializer(typeof(T));
            if (!File.Exists(path)) return default(T);
            var fs = new FileStream(path, FileMode.Open);
            var cReturn = (T)xs.Deserialize(fs);
            fs.Close();
            return cReturn;
        }

        public static int IndexOf<T>(this IEnumerable<T> list, Predicate<T> condition)
        {
            int i = -1;
            return list.Any(x => { i++; return condition(x); }) ? i : -1;
        }

        public static void CopyTo(this Stream source, Stream destination, int bufferSize, int count)
        {
            byte[] buffer = new byte[bufferSize];
            int read;

            while ((read = source.Read(buffer, 0, Math.Min(buffer.Length, count))) > 0)
            {
                destination.Write(buffer, 0, read);
                count -= read;
            }        
        }
    }
}
