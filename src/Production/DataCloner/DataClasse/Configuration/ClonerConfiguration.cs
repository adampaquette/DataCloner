using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    public class ClonerConfiguration
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        public List<ServerModifier> Servers { get; set; }

        public ClonerConfiguration()
        {
            Servers = new List<ServerModifier>();
        }
    }
}