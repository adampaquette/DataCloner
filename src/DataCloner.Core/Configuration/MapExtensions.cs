using System.Collections.Generic;

namespace DataCloner.Core.Configuration
{
    internal static class MapExtensions
    {
        public static Dictionary<SehemaIdentifier, SehemaIdentifier> ConvertToDictionnary(this MapFrom map)
        {
            return null;
            
            //if (map == null) return null;

            //var output = new Dictionary<ServerIdentifier, ServerIdentifier>();
            //foreach (var road in map.Roads)
            //{
            //    Variable configVar;

            //    Int16 serverSrc;
            //    if (!Int16.TryParse(road.ServerSrc, out serverSrc))
            //    {
            //        if (!road.ServerSrc.IsVariable())
            //            throw new Exception($"The value '{road.ServerSrc}' is not a valid variable in the map with id='{map.Id}'.");
            //        configVar = map.Variables.FirstOrDefault(v => v.Name == road.ServerSrc);
            //        if (configVar == null)
            //            throw new Exception($"The variable '{road.ServerSrc}' is not found in the map with id='{map.Id}'.");
            //        serverSrc = Int16.Parse(configVar.Value);
            //        configVar = null;
            //    }

            //    Int16 serverDst;
            //    if (!Int16.TryParse(road.ServerDst, out serverDst))
            //    {
            //        if (!road.ServerDst.IsVariable())
            //            throw new Exception($"The value '{road.ServerDst}' is not a valid variable in the map with id='{map.Id}'.");
            //        configVar = map.Variables.FirstOrDefault(v => v.Name == road.ServerDst);
            //        if (configVar == null)
            //            throw new Exception($"The variable '{road.ServerDst}' is not found in the map with id='{map.Id}'.");
            //        serverDst = Int16.Parse(configVar.Value);
            //        configVar = null;
            //    }

            //    string databaseSrc = road.DatabaseSrc;
            //    if (databaseSrc.IsVariable())
            //    {
            //        configVar = map.Variables.FirstOrDefault(v => v.Name == databaseSrc);
            //        if (configVar == null)
            //            throw new Exception($"The variable '{databaseSrc}' is not found in the map with id='{map.Id}'.");
            //        databaseSrc = configVar.Value;
            //    }
            //    configVar = null;

            //    string databaseDst = road.DatabaseDst;
            //    if (databaseDst.IsVariable())
            //    {
            //        configVar = map.Variables.FirstOrDefault(v => v.Name == databaseDst);
            //        if (configVar == null)
            //            throw new Exception($"The variable '{databaseDst}' is not found in the map with id='{map.Id}'.");
            //        databaseDst = configVar.Value;
            //    }
            //    configVar = null;

            //    string schemaSrc = road.SchemaSrc;
            //    if (schemaSrc.IsVariable())
            //    {
            //        configVar = map.Variables.FirstOrDefault(v => v.Name == schemaSrc);
            //        if (configVar == null)
            //            throw new Exception($"The variable '{schemaSrc}' is not found in the map with id='{map.Id}'.");
            //        schemaSrc = configVar.Value;
            //    }
            //    configVar = null;

            //    string schemaDst = road.SchemaDst;
            //    if (schemaDst.IsVariable())
            //    {
            //        configVar = map.Variables.FirstOrDefault(v => v.Name == schemaDst);
            //        if (configVar == null)
            //            throw new Exception($"The variable '{schemaDst}' is not found in the map with id='{map.Id}'.");
            //        schemaDst = configVar.Value;
            //    }
            //    configVar = null;

            //    output.Add(
            //        new ServerIdentifier { ServerId = serverSrc, Database = databaseSrc, Schema = schemaSrc },
            //        new ServerIdentifier { ServerId = serverDst, Database = databaseDst, Schema = schemaDst });
            //}
            //return output;
        }
    }
}
