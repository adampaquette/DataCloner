using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{SourceVar + \"_\" + DestinationVar}")]
    [Serializable]
    public class Road
    {
        [XmlAttribute]
        public string SourceVar { get; set; }

        [XmlAttribute]
        public string DestinationVar { get; set; }
    }
}
