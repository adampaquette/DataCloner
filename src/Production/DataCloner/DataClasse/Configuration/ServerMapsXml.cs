using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    [XmlRoot("ServersMaps")]
    public class ServersMapsXml
    {
        [XmlElement("Map")]
        public List<MapXml> Maps { get; set; }

        public ServersMapsXml()
        {
            Maps = new List<MapXml>();
        }

        public class MapXml
        {
            [XmlAttribute]
            public string nameFrom { get; set; }
            [XmlAttribute]
            public string nameTo { get; set; }
            [XmlElement("Road")]
            public List<RoadXml> Roads { get; set; }

            public MapXml()
            {
                Roads = new List<RoadXml>();
            }

            public static implicit operator Dictionary<ServerIdentifier, ServerIdentifier>(ServersMapsXml.MapXml map)
            {
                if (map == null) return null;

                var output = new Dictionary<ServerIdentifier, ServerIdentifier>();
                foreach (var road in map.Roads)
                {
                    output.Add(
                        new ServerIdentifier { ServerId = road.ServerSrc, Database = road.DatabaseSrc, Schema = road.SchemaSrc },
                        new ServerIdentifier { ServerId = road.ServerDst, Database = road.DatabaseDst, Schema = road.SchemaDst });
                }
                return output;
            }
        }

        public class RoadXml
        {
            [XmlAttribute]
            public Int16 ServerSrc { get; set; }
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