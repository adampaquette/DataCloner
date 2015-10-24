using System;
using System.Diagnostics;
using System.IO;

namespace DataCloner.Metadata
{
    [DebuggerDisplay("{ServerId.ToString() + \".\" + Database + \".\" + Schema}")]
    public struct ServerIdentifier
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
    }
}
