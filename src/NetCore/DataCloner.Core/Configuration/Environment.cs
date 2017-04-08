using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Name}")]
    [Serializable]
    public class Environment
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Schema")]
        public List<SchemaVar> Schemas { get; set; }

        public Environment()
        {
            Schemas = new List<SchemaVar>();
        }
    }
}