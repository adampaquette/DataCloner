using System;
using System.Data;
using DataCloner.DataClasse.Cache;
using DataCloner.Framework;

namespace DataCloner.PlugIn
{
    internal class StringDataBuilder : IDataBuilder
    {
        public object BuildData(IDbConnection conn, DbEngine engine, Int16 serverId, string database, string schema, ITableSchema table, IColumnDefinition column)
        {
            int size = column.SqlType.Precision != 0 ? column.SqlType.Precision : 10;

            return KeyGenerator.GetUniqueKey(size);
        }

        public void ClearCache()
        {
        }
    }
}


