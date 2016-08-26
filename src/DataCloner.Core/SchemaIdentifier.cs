using System;
using System.Diagnostics;
using System.IO;

namespace DataCloner.Core
{
    [DebuggerDisplay("{ServerId.ToString() + \".\" + Database + \".\" + Schema}")]
    public class SehemaIdentifier : IEquatable<SehemaIdentifier>
    {
        public Int16 ServerId { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }

        public void Serialize(BinaryWriter output)
        {
            output.Write(ServerId);
            output.Write(Database);
            output.Write(Schema);
        }

        public static SehemaIdentifier Deserialize(BinaryReader input)
        {
            return new SehemaIdentifier
            {
                ServerId = input.ReadInt16(),
                Database = input.ReadString(),
                Schema = input.ReadString()
            };
        }

        public override bool Equals(object obj)
        {
            var sv = obj as SehemaIdentifier;
            return sv != null && Equals(sv);
        }

        public override int GetHashCode()
        {
            return (ServerId + Database + Schema).GetHashCode();
        }

        public bool Equals(SehemaIdentifier other)
        {
            return other != null &&
                ServerId == other.ServerId &&
                Database == other.Database &&
                Schema == other.Schema;
        }
    }
}
