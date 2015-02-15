using System;
using System.Data;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;

namespace DataCloner.Tests
{
    internal static class Tools
    {
        internal static IRowIdentifier NewRi(Int16 serverId, string database,
            string schema, string table, ColumnsWithValue cols)
        {
            return new RowIdentifier
            {
                ServerId = serverId,
                Database = database,
                Schema = schema,
                Table = table,
                Columns = cols
            };
        }
    }
}