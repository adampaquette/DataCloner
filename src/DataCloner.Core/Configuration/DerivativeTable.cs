using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{DestinationVar + \"_\" + Name}")]
    [Serializable]
    public class DerivativeTable
    {
        [XmlAttribute]
        public string DestinationVar { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public DerivativeTableAccess Access { get; set; }

        [XmlAttribute]
        public NullableBool Cascade { get; set; }
    }
}
