using System;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    public class ConnectionXml
    {
        [XmlAttribute]
        public Int16 Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ProviderName { get; set; }
        [XmlAttribute]
        public string ConnectionString { get; set; }

        public ConnectionXml() { }
        public ConnectionXml(Int16 id, string name, string providerName, string connectionString)
        {
            Id = id;
            Name = name;
            ProviderName = providerName;
            ConnectionString = connectionString;
        }
    }
}
