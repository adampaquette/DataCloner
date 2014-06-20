using System;
using System.Xml.Serialization;
using System.IO;

namespace DataCloner.DataClasse.Configuration
{
    public class Connection
    {
        public Int16 Id { get; set; }
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
        public Int16 SameConfigAsId { get; set; }

        public Connection() { }
        public Connection(Int16 id, string providerName, string connectionString, Int16 sameConfigAsId)
        {
            Id = id;
            ProviderName = providerName;
            ConnectionString = connectionString;
            SameConfigAsId = sameConfigAsId;
        }

        public void Serialize(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Id);
            bw.Write(ProviderName);
            bw.Write(ConnectionString);
            bw.Write(SameConfigAsId);
        }

        public static Connection Deserialize(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            return new Connection()
            {
                Id = br.ReadInt16(),
                ProviderName = br.ReadString(),
                ConnectionString = br.ReadString(),
                SameConfigAsId = br.ReadInt16()
            };
        }
    }
}
