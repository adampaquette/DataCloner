using System;
using System.Data;
using DataCloner.DataClasse.Cache;

namespace DataCloner.PlugIn
{
    internal class AutoIncrementDataBuilder : IDataBuilder
    {
        public object BuildData(IDbConnection conn, DbEngine engine, string database, ITableSchema table, IColumnDefinition column)
        {
            switch (engine)
            {
                case DbEngine.MySql:
                    return GetNewKeyMySql(conn, database, table, column);
            }
            throw new NotSupportedException();
        }

        private object GetNewKeyMySql(IDbConnection conn, string database, ITableSchema table, IColumnDefinition column)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = String.Format("SELECT MAX({0})+1 FROM {1}.{2}", column.Name, database, table.Name);
            conn.Open();
            var result = cmd.ExecuteScalar();
            conn.Close();
            return result;
        }
    }
}
