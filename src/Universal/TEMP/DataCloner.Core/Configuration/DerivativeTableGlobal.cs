using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    
    public class DerivativeTableGlobal
    {
        [XmlAttribute]
        public DerivativeTableAccess GlobalAccess { get; set; }

        [XmlAttribute]
        public NullableBool GlobalCascade { get; set; }

        [XmlElement("Table")]
        public List<DerivativeTable> DerivativeTables { get; set; }

        public DerivativeTableGlobal()
        {
            DerivativeTables = new List<DerivativeTable>();
        }
    }
}
