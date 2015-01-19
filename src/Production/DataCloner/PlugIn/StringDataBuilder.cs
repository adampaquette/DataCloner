using System;
using DataCloner.DataClasse.Cache;
using System.Data;

namespace DataCloner.PlugIn
{
    internal class StringDataBuilder : IDataBuilder
    {
        public object BuildData(IDbConnection conn, ITableSchema table, IColumnDefinition column)
        {
            return Guid.NewGuid().ToString();
        }
    }
}


