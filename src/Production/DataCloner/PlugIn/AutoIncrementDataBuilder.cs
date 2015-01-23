using System;
using DataCloner.DataClasse.Cache;
using System.Data;

namespace DataCloner.PlugIn
{
    internal class AutoIncrementDataBuilder : IDataBuilder
    {
        public object BuildData(IDbConnection conn, DbEngine engine, ITableSchema table, IColumnDefinition column)
        {
            switch (engine)
            {
                case DbEngine.MySql:
                    return GetNewKeyMySql(conn, table, column);
            }
            throw new NotSupportedException();
        }

        private object GetNewKeyMySql(IDbConnection conn, ITableSchema table, IColumnDefinition column)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = String.Format("SELECT MAX({0})+1 FROM {1}", column.Name, table.Name);
            return cmd.ExecuteScalar();
        }
    }
}
