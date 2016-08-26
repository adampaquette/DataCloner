using System;
using System.Runtime.Serialization;

namespace DataCloner.Core.Configuration
{
    [DataContract]
    public class Connection
    {
        [DataMember]
        public Int16 Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ProviderName { get; set; }
        [DataMember]
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
    }
}