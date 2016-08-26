using DataCloner.Core.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace DataCloner.Core.Internal
{
    [DebuggerDisplay("SqlVar: Key={Id}, Value={Value}")]
    public class SqlVariable : IEquatable<SqlVariable>
    {
        public Int32 Id { get; }
        public object Value { get; set; }
        public bool QueryValue { get; set; }

        public SqlVariable(Int32 id)
        {
            Id = id;
            QueryValue = true;
        }

        public override bool Equals(object obj)
        {
            var sv = obj as SqlVariable;
            return sv?.Id == Id;
        }

        public bool Equals(SqlVariable other)
        {
            return other?.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void Serialize(BinaryWriter output)
        {
            output.Write(Id);
            output.Write(QueryValue);
            SerializationHelper.Serialize(output.BaseStream, Value);
        }

        public static SqlVariable Deserialize(BinaryReader input)
        {
            return new SqlVariable(input.ReadInt32())
            {
                QueryValue = input.ReadBoolean(),
                Value = SerializationHelper.Deserialize<Object>(input.BaseStream)
            };
        }
    }
}
