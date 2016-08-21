using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class DerivativeTableGlobal
    {
        [XmlAttribute]
        public DerivativeTableAccess? GlobalAccess { get; set; }

        [XmlAttribute]
        public bool? GlobalCascade { get; set; }

        [XmlElement("Table")]
        public List<DerivativeTable> DerivativeSubTables { get; set; }

        public DerivativeTableGlobal()
        {
            DerivativeSubTables = new List<DerivativeTable>();
        }
    }
}
