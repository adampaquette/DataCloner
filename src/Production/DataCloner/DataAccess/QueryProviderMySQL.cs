using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using DataCloner.DataClasse.Cache;
using DataCloner.Interface;
using MySql.Data.MySqlClient;
using IQueryProvider = DataCloner.Interface.IQueryProvider;

namespace DataCloner.DataAccess
{
    internal sealed class QueryProviderMySql : IQueryProvider
    {
        private readonly MySqlConnection _conn;
        private readonly Configuration _cache;
        private readonly Int16 _serverIdCtx;
        private readonly bool _isReadOnly;

        public QueryProviderMySql(string connectionString, Int16 serverId, Configuration cache)
        {
            _cache = cache;
            _serverIdCtx = serverId;
            _conn = new MySqlConnection(connectionString);
            _conn.Open();
        }

        public QueryProviderMySql(string connectionString, Int16 serverId, Configuration cache, bool readOnly)
            : this(connectionString, serverId, cache)
        {
            _isReadOnly = readOnly;
        }

        ~QueryProviderMySql()
        {
            Dispose(false);
        }

        public IDbConnection Connection
        {
            get { return _conn; }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public string[] GetDatabasesName()
        {
            List<string> databases = new List<string>();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA " +
                                  "WHERE SCHEMA_NAME NOT IN ('information_schema','performance_schema','mysql')";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        databases.Add(r.GetString(0));
                }
            }
            return databases.ToArray();
        }

        public void GetColumns(Action<IDataReader, Int16, string> reader, string database)
        {
            var sql =
                "SELECT " +
                    "'' AS SHEMA," +
                    "TABLE_NAME," +
                    "COLUMN_NAME," +
                    "DATA_TYPE," +
                    "COLUMN_KEY = 'PRI' AS 'IsPrimaryKey'," +
                    "1 AS 'IsForeignKey'," +
                    "EXTRA = 'auto_increment' AS 'IsAutoIncrement' " +
                "FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_SCHEMA = @DATABASE " +
                "ORDER BY " +
                    "TABLE_NAME," +
                    "ORDINAL_POSITION";

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@DATABASE", database);
                using (var r = cmd.ExecuteReader())
                {
                    reader(r, _serverIdCtx, database);
                }
            }
        }

        public void GetForeignKeys(Action<IDataReader, Int16, string> reader, string database)
        {
            var dtReturn = new DataTable();

            var sql =
                "SELECT " +
                    "'' AS 'Schema'," +
                    "TC.TABLE_NAME," +
                    "TC.CONSTRAINT_NAME," +
                    "K.COLUMN_NAME," +
                    "K.REFERENCED_TABLE_SCHEMA," +
                    "K.REFERENCED_TABLE_NAME," +
                    "K.REFERENCED_COLUMN_NAME " +
                "FROM information_schema.TABLE_CONSTRAINTS TC " +
                "INNER JOIN information_schema.KEY_COLUMN_USAGE K ON TC.TABLE_NAME = K.TABLE_NAME " +
                                                                "AND TC.CONSTRAINT_NAME = K.CONSTRAINT_NAME " +
                "WHERE TC.TABLE_SCHEMA = @DATABASE " +
                "AND TC.CONSTRAINT_TYPE = 'FOREIGN KEY' " +
                "ORDER BY " +
                    "TC.TABLE_NAME," +
                    "TC.CONSTRAINT_NAME";

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@DATABASE", database);
                using (var r = cmd.ExecuteReader())
                {
                    reader(r, _serverIdCtx, database);
                }
            }
        }

        public object GetLastInsertedPk()
        {
            var cmd = new MySqlCommand("SELECT LAST_INSERT_ID();", _conn);
            return cmd.ExecuteScalar();
        }

        public object[][] Select(IRowIdentifier ri)
        {
            List<object[]> rows = new List<object[]>();
            TableDef schema = _cache.CachedTables.GetTable(ri.ServerId, ri.Database, ri.Schema, ri.Table);
            StringBuilder query = new StringBuilder(schema.SelectCommand);
            int nbParams = ri.Columns.Count;

            using (var cmd = _conn.CreateCommand())
            {
                //Build query / params
                if (nbParams > 0)
                    query.Append(" WHERE ");

                for (int i = 0; i < nbParams; i++)
                {
                    string paramName = ri.Columns.ElementAt(i).Key;
                    query.Append(paramName).Append(" = @").Append(paramName);
                    cmd.Parameters.AddWithValue("@" + paramName, ri.Columns.ElementAt(i).Value);
                    if (i < nbParams - 1)
                        query.Append(" AND ");
                }
                cmd.CommandText = query.ToString();

                //Exec query
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        object[] values = new object[r.FieldCount];
                        r.GetValues(values);
                        rows.Add(values);
                    }
                }
            }

            if (rows != null)
                return rows.ToArray();
            return null;
        }

        public void Insert(ITableIdentifier ti, object[] row)
        {
            TableDef schema = _cache.CachedTables.GetTable(ti.ServerId, ti.Database, ti.Schema, ti.Table);
            if (schema.SchemaColumns.Count() != row.Length)
                throw new Exception("The row doesn't correspond to schema!");

            using (var cmd = _conn.CreateCommand())
            {
                //Add params
                for (int i = 0; i < schema.SchemaColumns.Count(); i++)
                {
                    if (schema.SchemaColumns[i].IsAutoIncrement) continue;
                    cmd.Parameters.AddWithValue("@" + schema.SchemaColumns[i].Name, row[i]);
                }
                cmd.CommandText = schema.InsertCommand;

                //Exec query
                var r = cmd.ExecuteNonQuery();
            }
        }

        public void Update(IRowIdentifier ri, DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        public void Delete(IRowIdentifier ri)
        {
            var cmd = new MySqlCommand();
            var sql = new StringBuilder("DELETE FROM ");
            sql.Append(ri.Database)
               .Append(".")
               .Append(ri.Table);

            if (ri.Columns.Count > 1)
                sql.Append(" WHERE 1=1");

            foreach (var kv in ri.Columns)
            {
                sql.Append(" AND ")
                   .Append(kv.Key)
                   .Append(" = @")
                   .Append(kv.Key);

                cmd.Parameters.AddWithValue("@" + kv.Key, kv.Value);
            }

            cmd.CommandText = sql.ToString();
            cmd.Connection = _conn;
            cmd.ExecuteNonQuery();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            //if (disposing)
            //{
            //    if (_conn != null)
            //    {
            //        if (_conn.State != ConnectionState.Closed)
            //            _conn.Close();
            //        _conn.Dispose();
            //        _conn = null;
            //    }
            //}
        }

        //public bool OpenConnection()
        //{
        //    try
        //    {
        //        _conn.Open();
        //        return true;
        //    }
        //    catch (MySqlException ex)
        //    {
        //        switch (ex.Number)
        //        {
        //            case 0:
        //                throw new MySqlException("Cannot connect to server", ex.Number);
        //                break;

        //            case 1042:
        //                MessageBox.Show("Unable to connect to any of the specified MySQL hosts");
        //                break;

        //            case 1045:
        //                MessageBox.Show("Invalid username/password");
        //                break;
        //        }
        //        return false;
        //    }
        //}

        public void Init()
        {
            throw new NotImplementedException();
        }
    }
}
