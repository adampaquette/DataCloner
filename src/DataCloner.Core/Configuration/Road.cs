using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Source + \"_\" + Destination}")]
    [Serializable]
    public class Road
    {
        [XmlAttribute]
        public string Source { get; set; }

        [XmlAttribute]
        public string Destination { get; set; }
    }
}
