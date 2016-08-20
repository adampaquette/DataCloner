using System;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class DerivativeSubTable
    {
        [XmlAttribute]
        public string Destination { get; set; }

        [XmlAttribute]
        public string Table { get; set; }

        [XmlAttribute]
        public DerivativeTableAccess Access { get; set; }

        [XmlAttribute]
        public bool Cascade { get; set; }
    }
}
