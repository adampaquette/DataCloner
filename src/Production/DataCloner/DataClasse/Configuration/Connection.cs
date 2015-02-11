using System;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class Connection
    {
        [XmlAttribute]
        public Int16 Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ProviderName { get; set; }
        [XmlAttribute]
        public string ConnectionString { get; set; }
        [XmlAttribute]
        public Int16 SameConfigAsId { get; set; }

        public Connection() { }
        public Connection(Int16 id, string name, string providerName, string connectionString, Int16 sameConfigAsId)
        {
            Id = id;
            Name = name;
            ProviderName = providerName;
            ConnectionString = connectionString;
            SameConfigAsId = sameConfigAsId;
        }
    }
}