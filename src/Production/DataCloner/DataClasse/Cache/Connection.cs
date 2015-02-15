using System;
using System.IO;

namespace DataCloner.DataClasse.Cache
{
    public class Connection
    {
        public Int16 Id { get; set; }
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }

        public Connection() { }
        public Connection(Int16 id, string providerName, string connectionString)
        {
            Id = id;
            ProviderName = providerName;
            ConnectionString = connectionString;
        }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        } 

        public static Connection Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(Id);
            stream.Write(ProviderName);
            stream.Write(ConnectionString);
        }  

        public static Connection Deserialize(BinaryReader stream)
        {
            return new Connection
            {
                Id = stream.ReadInt16(),
                ProviderName = stream.ReadString(),
                ConnectionString = stream.ReadString()
            };
        }
    }
}