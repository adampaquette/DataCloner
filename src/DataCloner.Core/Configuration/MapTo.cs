using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    public class MapTo
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Var")]
        public List<Variable> Variables { get; set; }
    }
}