using System.Data;
using DataCloner.Core.Metadata;

namespace DataCloner.Core.PlugIn
{
    public interface IDataBuilder
    {
        object BuildData(IDbTransaction transaction, DbEngine engine, short serverId, string database, string schema, TableMetadata table, ColumnDefinition column);
        void ClearCache();
    }
}
