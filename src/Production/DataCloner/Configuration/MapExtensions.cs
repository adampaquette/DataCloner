using DataCloner.Metadata;
using DataCloner.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner.Configuration
{
    static class MapExtensions
    {
        public static Dictionary<ServerIdentifier, ServerIdentifier> ConvertToDictionnary(this Map map)
        {
            if (map == null) return null;

            var output = new Dictionary<ServerIdentifier, ServerIdentifier>();
            foreach (var road in map.Roads)
            {
                Variable configVar;

                Int16 serverSrc;
                if (!Int16.TryParse(road.ServerSrc, out serverSrc))
                {
                    if (!road.ServerSrc.IsVariable())
                        throw new Exception(string.Format("The value '{0}' is not a valid variable in the map with id='{1}'.", road.ServerSrc, map.Id));
                    configVar = map.Variables.FirstOrDefault(v => v.Name == road.ServerSrc);
                    if (configVar == null)
                        throw new Exception(string.Format("The variable '{0}' is not found in the map with id='{1}'.", road.ServerSrc, map.Id));
                    serverSrc = Int16.Parse(configVar.Value);
                    configVar = null;
                }

                Int16 serverDst;
                if (!Int16.TryParse(road.ServerDst, out serverDst))
                {
                    if (!road.ServerDst.IsVariable())
                        throw new Exception(string.Format("The value '{0}' is not a valid variable in the map with id='{1}'.", road.ServerDst, map.Id));
                    configVar = map.Variables.FirstOrDefault(v => v.Name == road.ServerDst);
                    if (configVar == null)
                        throw new Exception(string.Format("The variable '{0}' is not found in the map with id='{1}'.", road.ServerDst, map.Id));
                    serverDst = Int16.Parse(configVar.Value);
                    configVar = null;
                }

                string databaseSrc = road.DatabaseSrc;
                if (databaseSrc.IsVariable())
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == databaseSrc);
                    if (configVar == null)
                        throw new Exception(string.Format("The variable '{0}' is not found in the map with id='{1}'.", databaseSrc, map.Id));
                    databaseSrc = configVar.Value;
                }
                configVar = null;

                string databaseDst = road.DatabaseDst;
                if (databaseDst.IsVariable())
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == databaseDst);
                    if (configVar == null)
                        throw new Exception(string.Format("The variable '{0}' is not found in the map with id='{1}'.", databaseDst, map.Id));
                    databaseDst = configVar.Value;
                }
                configVar = null;

                string schemaSrc = road.SchemaSrc;
                if (schemaSrc.IsVariable())
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == schemaSrc);
                    if (configVar == null)
                        throw new Exception(string.Format("The variable '{0}' is not found in the map with id='{1}'.", schemaSrc, map.Id));
                    schemaSrc = configVar.Value;
                }
                configVar = null;

                string schemaDst = road.SchemaDst;
                if (schemaDst.IsVariable())
                {
                    configVar = map.Variables.FirstOrDefault(v => v.Name == schemaDst);
                    if (configVar == null)
                        throw new Exception(string.Format("The variable '{0}' is not found in the map with id='{1}'.", schemaDst, map.Id));
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
}
