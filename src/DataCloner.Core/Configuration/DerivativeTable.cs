using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class DerivativeTable
    {
        [XmlAttribute]
        public DerivativeTableAccess GlobalAccess { get; set; }
        [XmlAttribute]
        public bool GlobalCascade { get; set; }
        [XmlElement("Table")]
        public List<DerivativeSubTable> DerivativeSubTables { get; set; }

        public DerivativeTable()
        {
            DerivativeSubTables = new List<DerivativeSubTable>();
        }
    }
}
