//using ProtoBuf;
//using System;
//using System.IO;
//using DataCloner.Framework;

//namespace DataCloner.Internal
//{
//    [ProtoContract]
//    public class SqlVariableSurrogate
//    {
//        [ProtoMember(1)]
//        public Int32 Id { get; set; }
//        [ProtoMember(2)]
//        public byte[] Value { get; set; }
//        [ProtoMember(3)]
//        public bool QueryValue { get; set; }

//        public static implicit operator SqlVariableSurrogate(SqlVariable obj)
//        {
//            return obj == null ? null : new SqlVariableSurrogate
//            {
//                Id = obj.Id,
//                QueryValue = obj.QueryValue,
//                Value = Serialize(obj.Value)
//            };
//        }

//        private static byte[] Serialize(object obj)
//        {
//            using (var ms = new MemoryStream())
//            {
//                var tag = SerializationHelper.TypeToTag[obj.GetType()];
//                Serializer.NonGeneric.SerializeWithLengthPrefix(ms, obj, PrefixStyle.Base128, tag);
//                return ms.ToArray();
//            }
//        }

//        public static implicit operator SqlVariable(SqlVariableSurrogate obj)
//        {
//            return obj == null ? null : new SqlVariable(obj.Id)
//            {
//                QueryValue = obj.QueryValue,
//                Value = Deserialize(obj.Value)
//            };
//        }

//        private static object Deserialize(byte[] obj)
//        {
//            using (var ms = new MemoryStream(obj))
//            {
//                object value = null;
//                Serializer.NonGeneric.TryDeserializeWithLengthPrefix(ms, PrefixStyle.Base128, 
//                    t=>SerializationHelper.TagToType[t], out value);
//                return value;
//            }
//        }

//        //private static byte[] Serialize(object obj)
//        //{
//        //    using (var ms = new MemoryStream())
//        //    using (var bs = new BinaryWriter(ms))
//        //    {
//        //        var bf = new BinaryFormatter();
//        //        var tag = SerializationHelper.TypeToTag[obj.GetType()];
//        //        bf.Serialize(ms, obj)

//        //        bs.Write(tag);
//        //        bs.Write();


//        //        return ms.ToArray();
//        //    }
//        //}
//    }
//}
