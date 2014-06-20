﻿using System;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    public class ConnectionXml
    {
        [XmlAttribute]
        public Int16 Id { get; set; }
        [XmlAttribute]
        public string ProviderName { get; set; }
        [XmlAttribute]
        public string ConnectionString { get; set; }
        [XmlAttribute]
        public Int16 SameConfigAsId { get; set; }

        public ConnectionXml() { }
        public ConnectionXml(Int16 id, string providerName, string connectionString, Int16 sameConfigAsId)
        {
            Id = id;
            ProviderName = providerName;
            ConnectionString = connectionString;
            SameConfigAsId = sameConfigAsId;
        }
    }
}
