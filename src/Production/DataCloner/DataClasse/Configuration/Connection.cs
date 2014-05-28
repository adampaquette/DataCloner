using System;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    public class Connection
    {
        public Int16 Id { get; set; }
        public string Name { get; set; }
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }

        public Connection(Int16 id, string name, string providerName, string connectionString)
        {
            Id = id;
            Name = name;
            ProviderName = providerName;
            ConnectionString = connectionString;
        }
    }
}
