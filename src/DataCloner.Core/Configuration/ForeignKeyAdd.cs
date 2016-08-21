using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class ForeignKeyAdd
    {
        [XmlAttribute]
        public string Destination { get; set; }

        [XmlAttribute]
        public string Table { get; set; }

        [XmlElement("Column")]
        public List<ForeignKeyColumn> Columns { get; set; }

        public ForeignKeyAdd()
        {
            Columns = new List<ForeignKeyColumn>();
        }
    }
}
