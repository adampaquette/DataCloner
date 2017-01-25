using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    
    public class ForeignKeys
    {
        [XmlElement("Add")]
        public List<ForeignKeyAdd> ForeignKeyAdd { get; set; }

        [XmlElement("Remove")]
        public ForeignKeyRemove ForeignKeyRemove { get; set; }

        public ForeignKeys()
        {
            ForeignKeyAdd = new List<ForeignKeyAdd>();
            ForeignKeyRemove = new ForeignKeyRemove();
        }
    }
}
