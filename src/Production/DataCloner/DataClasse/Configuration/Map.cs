using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class Map
    {
        [XmlAttribute]
        public string From { get; set; }
        [XmlAttribute]
        public string To { get; set; }
        [XmlAttribute]
        public string UsableConfigs { get; set; }
        [XmlElement("Var")]
        public List<Variable> Variables { get; set; }

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

    [Serializable]
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

    [Serializable]
    public class Variable
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Value { get; set; }
    }        
}