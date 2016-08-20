using System;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class ForeignKeyRemoveColumn
    {
        [XmlAttribute]
        public string Name { get; set; }
    }
}
