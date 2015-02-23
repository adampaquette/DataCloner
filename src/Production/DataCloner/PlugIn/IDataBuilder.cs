using System;
using System.Data;
using DataCloner.DataClasse.Cache;

namespace DataCloner.PlugIn
{
    public interface IDataBuilder
    {
        object BuildData(IDbConnection conn, DbEngine engine, Int16 serverId, string database, string schema, ITableSchema table, IColumnDefinition column);
        void ClearCache();
    }
}
