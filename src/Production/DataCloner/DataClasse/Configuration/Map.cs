using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataCloner.DataClasse.Cache;
using System.Linq;
using DataCloner.Framework;

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
        public string UsableBehaviours { get; set; }
        [XmlArrayItem("Var")]
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
                    if(!road.ServerSrc.IsVariable()) 
                        throw new Exception(string.Format("The value '{0}' is not a valid variable in the map id='{1}'.", road.ServerSrc, map.Id ));
                    configVar = map.Variables.FirstOrDefault(v => v.Name == road.ServerSrc);
                    if (configVar == null || !Int16.TryParse(road.ServerSrc, out serverSrc))
                        throw new Exception(string.Format("Variable '{0}' not found in the map id='{1}'.", road.ServerSrc, map.Id));
                    configVar = null;
                }

                Int16 serverDst;
                if (!Int16.TryParse(road.ServerDst, out serverDst))
                {
                    if (!road.ServerDst.IsVariable())
                        throw new Exception(string.Format("The value '{0}' is not a valid variable in the map id='{1}'.", road.ServerDst, map.Id));
                    configVar = map.Variables.FirstOrDefault(v => v.Name == road.ServerDst);
                    if (configVar == null || !Int16.TryParse(road.ServerDst, out serverDst))
                        throw new Exception(string.Format("Variable '{0}' not found in the map id='{1}'.", road.ServerDst, map.Id));
                    configVar = null;
                }

                string databaseSrc = road.DatabaseSrc;
                if (databaseSrc.IsVariable())
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == databaseSrc);
                    if (configVar == null)
                        throw new Exception(string.Format("Variable '{0}' not found in the map id='{1}'.", databaseSrc, map.Id));
                    databaseSrc = configVar.Value;
                }
                configVar = null;

                string databaseDst = road.DatabaseDst;
                if (databaseDst.IsVariable())
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == databaseDst);
                    if (configVar == null)
                        throw new Exception(string.Format("Variable '{0}' not found in the map id='{1}'.", databaseDst, map.Id));
                    databaseDst = configVar.Value;
                }
                configVar = null;

                string schemaSrc = road.SchemaSrc;
                if (schemaSrc.IsVariable())
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == schemaSrc);
                    if (configVar == null)
                        throw new Exception(string.Format("Variable '{0}' not found in the map id='{1}'.", schemaSrc, map.Id));
                    schemaSrc = configVar.Value;
                }
                configVar = null;

                string schemaDst = road.SchemaDst;
                if (schemaDst.IsVariable())
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == schemaDst);
                    if (configVar == null)
                        throw new Exception(string.Format("Variable '{0}' not found in the map id='{1}'.", schemaDst, map.Id));
                    schemaDst = configVar.Value;
                }
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