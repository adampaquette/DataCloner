using System.Data;
using DataCloner.Core.Metadata;
using DataCloner.Core.Framework;

namespace DataCloner.Core.PlugIn
{
    internal class StringDataBuilder : IDataBuilder
    {
        public object BuildData(IDbTransaction transaction, DbEngine engine, short serverId, string database, string schema, TableMetadata table, ColumnDefinition column)
        {
            var size = column.SqlType.Precision != 0 ? column.SqlType.Precision : 10;

            return KeyGenerator.GetUniqueKey(size);
        }

        public void ClearCache()
        {
        }
    }
}


