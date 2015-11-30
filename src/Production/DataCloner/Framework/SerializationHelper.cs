using DataCloner.Internal;
using DataCloner.Metadata;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataCloner.Framework
{
    public static class SerializationHelper
    {
        private static FastAccessList<Type> _tagToType = null;
        private static Dictionary<Type, int> _typeToTag = null;
        private static BinaryFormatter _bf = null;

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
                }
                return _typeToTag;
            }
        }

        public static BinaryFormatter DefaultFormatter
        {
            get
            {
                if (_bf == null)
                    _bf = new BinaryFormatter();
                return _bf;
            }
        }
    }
}
