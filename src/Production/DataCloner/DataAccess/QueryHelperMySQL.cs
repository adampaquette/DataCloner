using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using SqlFu;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataAccess
{
    internal sealed class QueryHelperMySql : IQueryHelper
    {
        private readonly IDbConnection _conn;
        private readonly Configuration _cache;
        private readonly Int16 _serverIdCtx;
        private readonly bool _isReadOnly;

        public QueryHelperMySql(string connectionString, Int16 serverId, Configuration cache)
        {
            _cache = cache;
            _serverIdCtx = serverId;
            _conn = new SqlFuConnection(connectionString, DbEngine.MySql);
            _conn.Open();
        }

        public QueryHelperMySql(string connectionString, Int16 serverId, Configuration cache, bool readOnly)
            : this(connectionString, serverId, cache)
        {
            _isReadOnly = readOnly;
        }

        ~QueryHelperMySql()
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

        public void GetColumns(ColumnReader reader, string database)
        {
            var sql =
                "SELECT " +
                    "'' AS SHEMA," +
                    "TABLE_NAME," +
                    "COLUMN_NAME," +
                    "COLUMN_TYPE," +
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

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                using (var r = cmd.ExecuteReader())
                {
                    reader(r, _serverIdCtx, database, SqlTypeToDbType);
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

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);
                //cmd.Parameters.AddWithValue("@DATABASE", database);

                using (var r = cmd.ExecuteReader())
                {
                    reader(r, _serverIdCtx, database);
                }
            }
        }

        public object GetLastInsertedPk()
        {
            var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT LAST_INSERT_ID();";
            return cmd.ExecuteScalar();
        }

        public object[][] Select(IRowIdentifier ri)
        {
            List<object[]> rows = new List<object[]>();
            TableDef schema = ri.GetTable();
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

                    var p = cmd.CreateParameter();
                    p.ParameterName = "@" + paramName;
                    p.Value = ri.Columns.ElementAt(i).Value;
                    cmd.Parameters.Add(p);

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
            TableDef schema = ti.GetTable();
            if (schema.SchemaColumns.Count() != row.Length)
                throw new Exception("The row doesn't correspond to schema!");

            using (var cmd = _conn.CreateCommand())
            {
                //Add params
                for (int i = 0; i < schema.SchemaColumns.Count(); i++)
                {
                    if (schema.SchemaColumns[i].IsAutoIncrement) continue;

                    var p = cmd.CreateParameter();
                    p.ParameterName = "@" + schema.SchemaColumns[i].Name;
                    p.Value = row[i];
                    cmd.Parameters.Add(p);
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
            var cmd = _conn.CreateCommand();
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

                var p = cmd.CreateParameter();
                p.ParameterName = "@" + kv.Key;
                p.Value = kv.Value;
                cmd.Parameters.Add(p);
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

        /// <summary>
        /// Détermine le DbType correspondant au type SQL sous les formes :
        /// smallint(5) unsigned, varchar(50), decimal(5,2), timestamp, enum('G','P','R').
        /// </summary>
        /// <param name="fullType">Type de la colonne SQL</param>
        /// <returns>Type DbType</returns>
        /// <seealso cref="http://kimbriggs.com/computers/computer-notes/mysql-notes/mysql-data-types-50.file"/>
        public DbType SqlTypeToDbType(string fullType)
        {
            fullType = fullType.ToLower();
            int startPosLength = fullType.IndexOf("(");
            int endPosLength = fullType.IndexOf(")");
            string[] values = fullType.Split(' ');
            string type;
            string descriptor;
            string[] descriptorValues = null;
            int length;
            int precision;
            bool? signedness = null;

            //S'il y a une description du type entre ()
            if (startPosLength > -1 && endPosLength > startPosLength)
            {
                descriptor = fullType.Substring(startPosLength + 1, endPosLength - startPosLength - 1);
                descriptorValues = descriptor.Split(',');
                type = values[0].Substring(0, startPosLength);
            }
            else
            {
                type = values[0];
            }

            if (values.Length > 1)
                signedness = values[1] == "signed";

            //Parse descriptior
            switch (type)
            {
                case "enum":
                case "set":
                    break; //Not supported
                default:
                    if (descriptorValues != null)
                    {
                        if (descriptorValues.Length > 1)
                            precision = Int32.Parse(descriptorValues[1]);
                        length = Int32.Parse(descriptorValues[0]);
                    }
                    break;
            }

            //From unsigned to CLR data type
            if (signedness.HasValue && !signedness.Value)
            {
                switch (type)
                {
                    case "tinyint":
                    case "smallint":
                    case "mediumint": //À vérifier
                        return DbType.Int32;
                    case "int":
                        return DbType.Int64;
                    case "bigint":
                        return DbType.Decimal;
                }
            }

            //From signed to CLR data type
            switch (type)
            {
                case "tinyint":
                    return DbType.Byte;
                case "smallint":
                case "year":
                    return DbType.Int16;
                case "mediumint":
                case "int":
                    return DbType.Int32;
                case "bigint":
                case "bit":
                    return DbType.Int64;
                case "float":
                    return DbType.Single;
                case "double":
                    return DbType.Double;
                case "decimal":
                    return DbType.Decimal;
                case "char":
                case "varchar":
                case "tinytext":
                case "text":
                case "mediumtext":
                case "longtext":
                case "binary":
                case "varbinary":
                    return DbType.String;
                case "tinyblob":
                case "blob":
                case "mediumblob":
                case "longblob":
                case "enum":
                case "set":
                    return DbType.Binary;
                case "date":
                case "datetime":
                    return DbType.DateTime;
                case "time":
                case "timestamp":
                    return DbType.Time;
            }

            return DbType.Object;
        }
    }
}
