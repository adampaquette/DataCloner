using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class ForeignKeyRemove
    {
        [XmlElement("Column")]
        public List<ForeignKeyRemoveColumn> Columns { get; set; }

        public ForeignKeyRemove()
        {
            Columns = new List<ForeignKeyRemoveColumn>();
        }
    }
}
