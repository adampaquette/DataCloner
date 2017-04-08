using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Id.ToString()}")]
    
    public class Connection : IEquatable<Connection>
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public string ProviderName { get; set; }
        [XmlAttribute]
        public string ConnectionString { get; set; }

        public Connection() { }
        public Connection(string id, string providerName, string connectionString)
        {
            Id = id;
            ProviderName = providerName;
            ConnectionString = connectionString;
        }

        public override string ToString()
        {
            return ProviderName + " " + Id.ToString();
        }

        public override bool Equals(object obj)
        {
            var o = obj as Connection;
            return Equals(o);
        }

        public bool Equals(Connection other)
        {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream, Encoding.UTF8, true));
        }

        public static Connection Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream, Encoding.UTF8, true));
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(Id);
            stream.Write(ProviderName);
            stream.Write(ConnectionString);
        }

        public static Connection Deserialize(BinaryReader stream)
        {
            return new Connection(stream.ReadString(), stream.ReadString(),stream.ReadString());
        }
    }
}