using System;
using DataCloner.DataClasse.Cache;
using System.Data;

namespace DataCloner.PlugIn
{
    internal class AutoIncrementDataBuilder : IDataBuilder
    {
        public object BuildData(IDbConnection conn, ITableDef table, ISchemaColumn column)
        {
            return Int32.MaxValue;
        }
    }
}
