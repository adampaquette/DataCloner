using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataCloner.DataClasse.Cache;
using System.Linq;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class Map
    {
        [XmlAttribute]
        public int Id { get; set; }
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
            Variables = new List<Variable>();
            Roads = new List<Road>();
        }

        public static implicit operator Dictionary<ServerIdentifier, ServerIdentifier>(Map map)
        {
            if (map == null) return null;

            var output = new Dictionary<ServerIdentifier, ServerIdentifier>();
            foreach (var road in map.Roads)
            {
                Variable configVar;

                Int16 serverSrc;
                if (!Int16.TryParse(road.ServerSrc, out serverSrc))
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == road.ServerSrc);
                    if (configVar == null || !Int16.TryParse(road.ServerSrc, out serverSrc))
                        throw new Exception($"Variable '{road.ServerSrc}' not found in the map id='{map.Id}'.");
                    configVar = null;
                }

                Int16 serverDst;
                if (!Int16.TryParse(road.DatabaseSrc, out serverDst))
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == road.ServerDst);
                    if (configVar == null || !Int16.TryParse(road.ServerDst, out serverDst))
                        throw new Exception($"Variable '{road.ServerDst}' not found in the map id='{map.Id}'.");
                    configVar = null;
                }

                string databaseSrc;
                configVar = map.Variables.FirstOrDefault(v => v.Name == road.DatabaseSrc);
                if (configVar == null)
                    throw new Exception($"Variable '{road.DatabaseSrc}' not found in the map id='{map.Id}'.");
                databaseSrc = configVar.Value;
                configVar = null;

                string databaseDst;
                configVar = map.Variables.FirstOrDefault(v => v.Name == road.DatabaseDst);
                if (configVar == null)
                    throw new Exception($"Variable '{road.DatabaseDst}' not found in the map id='{map.Id}'.");
                databaseDst = configVar.Value;
                configVar = null;

                string schemaSrc;
                configVar = map.Variables.FirstOrDefault(v => v.Name == road.SchemaSrc);
                if (configVar == null)
                    throw new Exception($"Variable '{road.SchemaSrc}' not found in the map id='{map.Id}'.");
                schemaSrc = configVar.Value;
                configVar = null;

                string schemaDst;
                configVar = map.Variables.FirstOrDefault(v => v.Name == road.SchemaDst);
                if (configVar == null)
                    throw new Exception($"Variable '{road.SchemaDst}' not found in the map id='{map.Id}'.");
                schemaDst = configVar.Value;
                configVar = null;

                output.Add(
                    new ServerIdentifier { ServerId = serverSrc, Database = databaseSrc, Schema = schemaSrc },
                    new ServerIdentifier { ServerId = serverDst, Database = databaseDst, Schema = schemaDst });
            }
            return output;
        }
    }

    [Serializable]
    public class Road
    {
        [XmlAttribute]
        public string ServerSrc { get; set; }
        [XmlAttribute]
        public string DatabaseSrc { get; set; }
        [XmlAttribute]
        public string SchemaSrc { get; set; }
        [XmlAttribute]
        public string ServerDst { get; set; }
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