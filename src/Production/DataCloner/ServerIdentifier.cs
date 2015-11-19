using System;
using System.Diagnostics;
using System.IO;

namespace DataCloner
{
    [DebuggerDisplay("{ServerId.ToString() + \".\" + Database + \".\" + Schema}")]
    public class ServerIdentifier : IEquatable<ServerIdentifier>
    {
        public Int16 ServerId { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        }

        public static ServerIdentifier Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(ServerId);
            stream.Write(Database);
            stream.Write(Schema);
        }

        public static ServerIdentifier Deserialize(BinaryReader stream)
        {
            return new ServerIdentifier
            {
                ServerId = stream.ReadInt16(),
                Database = stream.ReadString(),
                Schema = stream.ReadString()
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
