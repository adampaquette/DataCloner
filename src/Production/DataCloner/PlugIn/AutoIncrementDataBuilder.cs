using System;
using System.Collections.Generic;
using System.Data;
using DataCloner.Metadata;

namespace DataCloner.PlugIn
{
    internal class AutoIncrementDataBuilder : IDataBuilder
    {
        private static readonly Dictionary<string, object> AutoIncrementCache = new Dictionary<string, object>();

        public object BuildData(IDbConnection conn, DbEngine engine, Int16 serverId, string database, string schema, ITableMetadata table, IColumnDefinition column)
        {
            var cacheId = serverId + database + schema + table.Name + column.Name;

            if (AutoIncrementCache.ContainsKey(cacheId))
                IncrementNumber(cacheId);
            else
            {
                object value;

                switch (engine)
                {
                    case DbEngine.MySql:
                        value = GetNewKeyMySql(conn, database, table, column);
                        break;
                    case DbEngine.SqlServer:
                        value = GetNewKeyMsSql(conn, database, schema, table, column);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                AutoIncrementCache.Add(cacheId, value);
            }
            return AutoIncrementCache[cacheId];
        }

        private static void IncrementNumber(string cacheId)
        {
            var t = AutoIncrementCache[cacheId].GetType();
            if (t == typeof(Int16))
            {
                var n = (Int16)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(Int32))
            {
                var n = (Int32)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(Int64))
            {
                var n = (Int64)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(UInt16))
            {
                var n = (UInt16)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(UInt32))
            {
                var n = (UInt32)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(UInt64))
            {
                var n = (UInt64)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
        }

        private object GetNewKeyMySql(IDbConnection conn, string database, ITableMetadata table, IColumnDefinition column)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("SELECT MAX({0})+1 FROM {1}.{2}", column.Name, database, table.Name);
            var result = cmd.ExecuteScalar();
            return result;
        }

        private object GetNewKeyMsSql(IDbConnection conn, string database, string schema, ITableMetadata table, IColumnDefinition column)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("SELECT MAX({0})+1 FROM {1}.{2}.{3}", column.Name, database, schema, table.Name);
            var result = cmd.ExecuteScalar();
            return result;
        }

        public void ClearCache()
        {
            AutoIncrementCache.Clear();
        }
    }
}
