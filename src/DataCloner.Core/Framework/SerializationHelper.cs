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
        private static FastAccessList<Type> _tagToType = null;
        private static Dictionary<Type, int> _typeToTag = null;

        public static FastAccessList<Type> TagToType
        {
            get
            {
                if (_tagToType == null)
                {
                    _tagToType = new FastAccessList<Type>();
                    _tagToType.Add(typeof(Int16));
                    _tagToType.Add(typeof(Int32));
                    _tagToType.Add(typeof(Int64));
                    _tagToType.Add(typeof(UInt16));
                    _tagToType.Add(typeof(UInt32));
                    _tagToType.Add(typeof(UInt64));
                    _tagToType.Add(typeof(IntPtr));
                    _tagToType.Add(typeof(UIntPtr));
                    _tagToType.Add(typeof(Boolean));
                    _tagToType.Add(typeof(Byte));
                    _tagToType.Add(typeof(SByte));
                    _tagToType.Add(typeof(Char));
                    _tagToType.Add(typeof(DateTime));
                    _tagToType.Add(typeof(DateTimeOffset));
                    _tagToType.Add(typeof(Decimal));
                    _tagToType.Add(typeof(Double));
                    _tagToType.Add(typeof(Guid));
                    _tagToType.Add(typeof(Single));
                    _tagToType.Add(typeof(String));
                    _tagToType.Add(typeof(SqlVariable));
                    _tagToType.Add(typeof(TableMetadata));
                    _tagToType.Add(typeof(DBNull));
                    _tagToType.Add(typeof(Byte[]));
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
                    _typeToTag = new Dictionary<Type, int>();
                    _typeToTag.Add(typeof(Int16), 0);
                    _typeToTag.Add(typeof(Int32), 1);
                    _typeToTag.Add(typeof(Int64), 2);
                    _typeToTag.Add(typeof(UInt16), 3);
                    _typeToTag.Add(typeof(UInt32), 4);
                    _typeToTag.Add(typeof(UInt64), 5);
                    _typeToTag.Add(typeof(IntPtr), 6);
                    _typeToTag.Add(typeof(UIntPtr), 7);
                    _typeToTag.Add(typeof(Boolean), 8);
                    _typeToTag.Add(typeof(Byte), 9);
                    _typeToTag.Add(typeof(SByte), 10);
                    _typeToTag.Add(typeof(Char), 11);
                    _typeToTag.Add(typeof(DateTime), 12);
                    _typeToTag.Add(typeof(DateTimeOffset), 13);
                    _typeToTag.Add(typeof(Decimal), 14);
                    _typeToTag.Add(typeof(Double), 15);
                    _typeToTag.Add(typeof(Guid), 16);
                    _typeToTag.Add(typeof(Single), 17);
                    _typeToTag.Add(typeof(String), 18);
                    _typeToTag.Add(typeof(SqlVariable), 19);
                    _typeToTag.Add(typeof(TableMetadata), 20);
                    _typeToTag.Add(typeof(DBNull), 21);
                    _typeToTag.Add(typeof(Byte[]), 22);
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
            return Task.Run<T>(() =>
            {
                if (!File.Exists(path)) return default(T);
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var xs = new XmlSerializer(typeof(T));
                    return (T)xs.Deserialize(fs);
                };
            });
        }
    }
}
