using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Name}")]
    [Serializable]
    public class MapTo
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement("Var")]
        public List<Variable> Variables { get; set; }
    }
}