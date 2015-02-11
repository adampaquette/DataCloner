using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace DataCloner.DataClasse.Configuration
{
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

        public static implicit operator Dictionary<ServerIdentifier, ServerIdentifier>(Map map)
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

    public class Road
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