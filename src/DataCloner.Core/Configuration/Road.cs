using System;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{

    [Serializable]
    public class Road
    {
        [XmlAttribute]
        public string Source { get; set; }

        [XmlAttribute]
        public string Destination { get; set; }
    }
}
