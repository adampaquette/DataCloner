using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class Modifiers
    {
        [XmlElement("Server")]
        public List<Server> Servers { get; set; }
        [XmlElement("Database")]
        public List<Database> DatabaseModifiers { get; set; }
        [XmlElement("Schema")]
        public List<Schema> SchemaModifiers { get; set; }

        public Modifiers()
        {
            Servers = new List<Server>();
            DatabaseModifiers = new List<Database>();
            SchemaModifiers = new List<Schema>();
        }
    }
 }