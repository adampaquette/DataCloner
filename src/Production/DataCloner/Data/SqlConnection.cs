using System;
using System.IO;

namespace DataCloner.Data
{
    public class SqlConnection
    {
        public Int16 Id { get; set; }
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }

        public SqlConnection() { }
        public SqlConnection(Int16 id, string providerName, string connectionString)
        {
            Id = id;
            ProviderName = providerName;
            ConnectionString = connectionString;
        }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        } 

        public static SqlConnection Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(Id);
            stream.Write(ProviderName);
            stream.Write(ConnectionString);
        }  

        public static SqlConnection Deserialize(BinaryReader stream)
        {
            return new SqlConnection
            {
                Id = stream.ReadInt16(),
                ProviderName = stream.ReadString(),
                ConnectionString = stream.ReadString()
            };
        }
    }
}