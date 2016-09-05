using System;
using System.Collections.Generic;
using System.Data;
using DataCloner.Core.Metadata;

namespace DataCloner.Core.PlugIn
{
    internal class AutoIncrementDataBuilder : IDataBuilder
    {
        private static readonly Dictionary<string, object> AutoIncrementCache = new Dictionary<string, object>();

        public object BuildData(IDbTransaction transaction, DbEngine engine, short serverId, string database, string schema, TableMetadata table, ColumnDefinition column)
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
                        value = GetNewKeyMySql(transaction, database, table, column);
                        break;
                    case DbEngine.SqlServer:
                        value = GetNewKeyMsSql(transaction, database, schema, table, column);
                        break;
                    case DbEngine.PostgreSql:
                        value = GetNewKeyPostgreSql(transaction, database, schema, table, column);
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
            if (t == typeof(short))
            {
                var n = (short)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(int))
            {
                var n = (int)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(long))
            {
                var n = (long)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(ushort))
            {
                var n = (ushort)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(uint))
            {
                var n = (uint)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
            else if (t == typeof(ulong))
            {
                var n = (ulong)AutoIncrementCache[cacheId];
                AutoIncrementCache[cacheId] = ++n;
            }
        }

        private static object GetNewKeyMySql(IDbTransaction transaction, string database, TableMetadata table, ColumnDefinition column)
        {
            var cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"SELECT MAX({column.Name})+1 FROM {database}.{table.Name}";
            var result = cmd.ExecuteScalar();
            return result;
        }

        private static object GetNewKeyMsSql(IDbTransaction transaction, string database, string schema, TableMetadata table, ColumnDefinition column)
        {
            var cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"SELECT MAX({column.Name})+1 FROM {database}.{schema}.{table.Name}";
            var result = cmd.ExecuteScalar();
            return result;
        }

        private static object GetNewKeyPostgreSql(IDbTransaction transaction, string database, string schema, TableMetadata table, ColumnDefinition column)
        {
            var cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"SELECT MAX(\"{column.Name}\")+1 FROM {schema}.\"{table.Name}\"";
            var result = cmd.ExecuteScalar();
            return result;
        }

        public void ClearCache()
        {
            AutoIncrementCache.Clear();
        }
    }
}
