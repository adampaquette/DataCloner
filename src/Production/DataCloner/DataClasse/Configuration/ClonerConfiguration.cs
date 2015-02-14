using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class ClonerConfiguration
    {
        [XmlAttribute]
        public Int16 Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlElement("Server")]
        public List<ServerModifier> Servers { get; set; }

        public ClonerConfiguration()
        {
            Servers = new List<ServerModifier>();
        }
    }
}