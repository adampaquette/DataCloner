using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DataCloner
{
    [DebuggerDisplay("{ServerId.ToString() + \".\" + Database + \".\" + Schema}")]
    public class ServerIdentifier : IEquatable<ServerIdentifier>
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

        public static ServerIdentifier Deserialize(BinaryReader input)
        {
            return new ServerIdentifier
            {
                ServerId = input.ReadInt16(),
                Database = input.ReadString(),
                Schema = input.ReadString()
            };
        }

        public override bool Equals(object obj)
        {
            var sv = obj as ServerIdentifier;
            if (sv != null)
                return Equals(sv);
            return false;
        }

        public override int GetHashCode()
        {
            return (ServerId + Database + Schema).GetHashCode();
        }

        public bool Equals(ServerIdentifier other)
        {
            return other != null &&
                ServerId == other.ServerId &&
                Database == other.Database &&
                Schema == other.Schema;
        }
    }
}
