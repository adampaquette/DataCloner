using System;
using System.Data;
using DataCloner.Metadata;

namespace DataCloner.PlugIn
{
    public interface IDataBuilder
    {
        object BuildData(IDbConnection conn, IDbTransaction transaction, DbEngine engine, Int16 serverId, string database, string schema, TableMetadata table, ColumnDefinition column);
        void ClearCache();
    }
}
