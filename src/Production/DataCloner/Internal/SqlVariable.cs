﻿using DataCloner.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace DataCloner.Internal
{
    [DebuggerDisplay("SqlVar: Key={Id}, Value={Value}")]
    public class SqlVariable : IEquatable<SqlVariable>
    {
        private Int32 _id;

        public Int32 Id
        {
            get { return _id; }
        }
        public object Value { get; set; }
        public bool QueryValue { get; set; }

        public SqlVariable(Int32 id)
        {
            _id = id;
            QueryValue = true;
        }

        public override bool Equals(object obj)
        {
            var sv = obj as SqlVariable;
            if (sv != null)
                return sv.Id == Id;
            return false;
        }

        public bool Equals(SqlVariable other)
        {
            if (other != null)
                return other.Id == Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void Serialize(BinaryWriter output)
        {
            var bf = SerializationHelper.DefaultFormatter;
            output.Write(Id);
            output.Write(QueryValue);
            bf.Serialize(output.BaseStream, Value);
        }

        public static SqlVariable Deserialize(BinaryReader input)
        {
            var bf = SerializationHelper.DefaultFormatter;
            return new SqlVariable(input.ReadInt32())
            {
                QueryValue = input.ReadBoolean(),
                Value = bf.Deserialize(input.BaseStream)
            };
        }
    }
}
