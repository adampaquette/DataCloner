using DataCloner.Internal;
using DataCloner.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataCloner.Framework
{
    public static class SerializationHelper
    {
        private static Dictionary<int, Type> _tagToType = null;
        private static Dictionary<Type, int> _typeToTag = null;

        public static Dictionary<int, Type> TagToType
        {
            get
            {
                if (_tagToType == null)
                {
                    _tagToType = new Dictionary<int, Type>();
                    _tagToType.Add(1, typeof(Int16));
                    _tagToType.Add(2, typeof(Int32));
                    _tagToType.Add(3, typeof(Int64));
                    _tagToType.Add(4, typeof(UInt16));
                    _tagToType.Add(5, typeof(UInt32));
                    _tagToType.Add(6, typeof(UInt64));
                    _tagToType.Add(7, typeof(IntPtr));
                    _tagToType.Add(8, typeof(UIntPtr));
                    _tagToType.Add(9, typeof(Boolean));
                    _tagToType.Add(10, typeof(Byte));
                    _tagToType.Add(11, typeof(SByte));
                    _tagToType.Add(12, typeof(Char));
                    _tagToType.Add(13, typeof(DateTime));
                    _tagToType.Add(14, typeof(DateTimeOffset));
                    _tagToType.Add(15, typeof(Decimal));
                    _tagToType.Add(16, typeof(Double));
                    _tagToType.Add(17, typeof(Guid));
                    _tagToType.Add(18, typeof(Single));
                    _tagToType.Add(19, typeof(String));
                    _tagToType.Add(20, typeof(SqlVariable));
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
                    _typeToTag.Add(typeof(Int16), 1);
                    _typeToTag.Add(typeof(Int32), 2);
                    _typeToTag.Add(typeof(Int64), 3);
                    _typeToTag.Add(typeof(UInt16), 4);
                    _typeToTag.Add(typeof(UInt32), 5);
                    _typeToTag.Add(typeof(UInt64), 6);
                    _typeToTag.Add(typeof(IntPtr), 7);
                    _typeToTag.Add(typeof(UIntPtr), 8);
                    _typeToTag.Add(typeof(Boolean), 9);
                    _typeToTag.Add(typeof(Byte), 10);
                    _typeToTag.Add(typeof(SByte), 11);
                    _typeToTag.Add(typeof(Char), 12);
                    _typeToTag.Add(typeof(DateTime), 13);
                    _typeToTag.Add(typeof(DateTimeOffset), 14);
                    _typeToTag.Add(typeof(Decimal), 15);
                    _typeToTag.Add(typeof(Double), 16);
                    _typeToTag.Add(typeof(Guid), 17);
                    _typeToTag.Add(typeof(Single), 18);
                    _typeToTag.Add(typeof(String), 19);
                    _typeToTag.Add(typeof(SqlVariable), 20);
                }
                return _typeToTag;
            }
        }

        //public static void SerializeObject(Stream output, object obj)
        //{
        //    var type = obj.GetType();
        //    var tag = SerializationHelper.TypeToTag[type];
        //    output.Write(tag);
        //}

        public static void SerializeObject(BinaryWriter output, object obj)
        {
            var bf = new BinaryFormatter();
            var type = obj.GetType();
            var tag = SerializationHelper.TypeToTag[type];
            output.Write(tag);
            bf.Serialize(output.BaseStream, obj);
        }
    }
}
