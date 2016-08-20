using System;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class Connection : IEquatable<Connection>
    {
        [XmlAttribute]
        public Int16 Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ProviderName { get; set; }
        [XmlAttribute]
        public string ConnectionString { get; set; }

        public Connection() { }
        public Connection(Int16 id, string name, string providerName, string connectionString)
        {
            Id = id;
            Name = name;
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
    }
}