using System;
using System.IO;
using System.Text;

namespace DataCloner.Core.Data
{
    public class SqlConnection : IEquatable<SqlConnection>
    {
        public Int16 Id { get; }
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }

        public SqlConnection(Int16 id)
        {
            Id = id;
        }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream, Encoding.UTF8, true));
        }

        public static SqlConnection Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream, Encoding.UTF8, true));
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(Id);
            stream.Write(ProviderName);
            stream.Write(ConnectionString);
        }

        public static SqlConnection Deserialize(BinaryReader stream)
        {
            return new SqlConnection(stream.ReadInt16())
            {
                ProviderName = stream.ReadString(),
                ConnectionString = stream.ReadString()
            };
        }

        public override bool Equals(object obj)
        {
            var con = obj as SqlConnection;
            return Equals(con);
        }

        public bool Equals(SqlConnection other)
        {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}