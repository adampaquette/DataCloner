using DataCloner.Core.Internal;
using DataCloner.Core.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DataCloner.Core.Framework
{
    public static class SerializationHelper
    {
        private static FastAccessList<Type> _tagToType;
        private static Dictionary<Type, int> _typeToTag;

        public static FastAccessList<Type> TagToType
        {
            get
            {
                if (_tagToType == null)
                {
                    _tagToType = new FastAccessList<Type>();
                    _tagToType.Add(typeof(short));
                    _tagToType.Add(typeof(int));
                    _tagToType.Add(typeof(long));
                    _tagToType.Add(typeof(ushort));
                    _tagToType.Add(typeof(uint));
                    _tagToType.Add(typeof(ulong));
                    _tagToType.Add(typeof(IntPtr));
                    _tagToType.Add(typeof(UIntPtr));
                    _tagToType.Add(typeof(bool));
                    _tagToType.Add(typeof(byte));
                    _tagToType.Add(typeof(sbyte));
                    _tagToType.Add(typeof(char));
                    _tagToType.Add(typeof(DateTime));
                    _tagToType.Add(typeof(DateTimeOffset));
                    _tagToType.Add(typeof(decimal));
                    _tagToType.Add(typeof(double));
                    _tagToType.Add(typeof(Guid));
                    _tagToType.Add(typeof(float));
                    _tagToType.Add(typeof(string));
                    _tagToType.Add(typeof(SqlVariable));
                    _tagToType.Add(typeof(TableMetadata));
                    _tagToType.Add(typeof(DBNull));
                    _tagToType.Add(typeof(byte[]));
                }
                return _tagToType;
            }
        }
        public static Dictionary<Type, int> TypeToTag
        {
            get
            {
                if (_typeToTag == null)
                {
                    _typeToTag = new Dictionary<Type, int>
                    {
                        {typeof(short), 0},
                        {typeof(int), 1},
                        {typeof(long), 2},
                        {typeof(ushort), 3},
                        {typeof(uint), 4},
                        {typeof(ulong), 5},
                        {typeof(IntPtr), 6},
                        {typeof(UIntPtr), 7},
                        {typeof(bool), 8},
                        {typeof(byte), 9},
                        {typeof(sbyte), 10},
                        {typeof(char), 11},
                        {typeof(DateTime), 12},
                        {typeof(DateTimeOffset), 13},
                        {typeof(decimal), 14},
                        {typeof(double), 15},
                        {typeof(Guid), 16},
                        {typeof(float), 17},
                        {typeof(string), 18},
                        {typeof(SqlVariable), 19},
                        {typeof(TableMetadata), 20},
                        {typeof(DBNull), 21},
                        {typeof(byte[]), 22}
                    };
                }
                return _typeToTag;
            }
        }

        public static void Serialize<T>(Stream output, T obj)
        {
            var xs = new DataContractSerializer(obj.GetType());
            xs.WriteObject(output, obj);
        }

        public static T Deserialize<T>(Stream input)
        {
            var xs = new DataContractSerializer(typeof(T));
            return (T)xs.ReadObject(input);
        }

        public static string SerializeXml<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            using (var sr = new StreamReader(ms))
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                var xws = new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = new string(' ', 4),
                };

                var xw = XmlWriter.Create(ms, xws);
                var xs = new XmlSerializer(obj.GetType());
                xs.Serialize(xw, obj, ns);
                ms.Position = 0;

                return sr.ReadToEnd();
            }
        }

        public static T DeserializeXml<T>(this string str)
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                sw.Write(str);
                ms.Position = 0;

                var xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(ms);
            }
        }

        public static void SaveXml<T>(this T obj, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                var xws = new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = new string(' ', 4),
                };

                var xw = XmlWriter.Create(fs, xws);
                var xs = new XmlSerializer(obj.GetType());
                xs.Serialize(xw, obj, ns);
            }
        }

        public static Task<T> LoadXmlAsync<T>(string path)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(path)) return default(T);
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var xs = new XmlSerializer(typeof(T));
                    return (T)xs.Deserialize(fs);
                }
            });
        }
    }
}
