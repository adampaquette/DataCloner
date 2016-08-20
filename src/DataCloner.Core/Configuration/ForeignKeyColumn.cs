using System;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class ForeignKeyColumn
    {
        [XmlAttribute]
        public string NameFrom { get; set; }
        [XmlAttribute]
        public string NameTo { get; set; }
    }
}
