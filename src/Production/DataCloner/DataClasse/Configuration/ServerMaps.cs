using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace DataCloner.DataClasse.Configuration
{
    public class ServersMaps
    {
    [XmlElement("Map")]
        public List<Map> Maps { get; set; }

        public ServersMaps()
        {
            Maps = new List<Map>();
        }

        public class Map
        {
            [XmlAttribute]
            public string nameFrom { get; set; }
            [XmlAttribute]
            public string nameTo { get; set; }
            [XmlElement("Road")]
            public List<Road> Roads { get; set; }

            public Map() 
            {
                Roads = new List<Road>();
            }
        }

        public class Road
        { 
            [XmlAttribute]
            public Int16 ServerSrc {get;set;}
            [XmlAttribute]
            public string DatabaseSrc { get; set; }
            [XmlAttribute]
            public string SchemaSrc { get; set; }
            [XmlAttribute]
            public Int16 ServerDst { get; set; }
            [XmlAttribute]
            public string DatabaseDst { get; set; }
            [XmlAttribute]
            public string SchemaDst { get; set; }
        }
    }
}
