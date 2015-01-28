using System;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using DataCloner.DataClasse.Cache;
using System.Data.Common;

namespace DataCloner.DataAccess
{
    internal class AbstractQueryHelper : IQueryHelper
    {
        private readonly DbProviderFactory _factory;
        private readonly IDbConnection _conn;
        private readonly Configuration _cache;
        private readonly Int16 _serverIdCtx;
        private readonly string _sqlGetDatabasesName;
        private readonly string _sqlGetColumns;
        private readonly string _sqlGetForeignKeys;
        private readonly string _sqlGetUniqueKeys;
        private readonly string _sqlGetLastInsertedPk;
        private readonly string _sqlEnforceIntegrityCheck;

        public AbstractQueryHelper(string providerName, string connectionString, Int16 serverId, Configuration cache,
            string sqlGetDatabasesName, string sqlGetColumns, string sqlGetForeignKeys, string sqlGetUniqueKeys,
            string sqlGetLastInsertedPk, string sqlEnforceIntegrityCheck)
        {
            _factory = DbProviderFactories.GetFactory(providerName);
            _conn = _factory.CreateConnection();
            _conn.ConnectionString = connectionString;
            _conn.Open();

            _cache = cache;
            _serverIdCtx = serverId;

            _sqlGetDatabasesName = sqlGetDatabasesName;
            _sqlGetColumns = sqlGetColumns;
            _sqlGetForeignKeys = sqlGetForeignKeys;
            _sqlGetUniqueKeys = sqlGetUniqueKeys;
            _sqlGetLastInsertedPk = sqlGetLastInsertedPk;
            _sqlEnforceIntegrityCheck = sqlEnforceIntegrityCheck;
        }

        public IDbConnection Connection
        {
            get { return _conn; }
        }

        public DbEngine Engine
        {
            get { return DbEngine.MySql; }
        }

        public string[] GetDatabasesName()
        {
            List<string> databases = new List<string>();

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = _sqlGetDatabasesName;
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
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = _sqlGetColumns;

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                using (var r = cmd.ExecuteReader())
                    reader(r, _serverIdCtx, database, SqlTypeToDbType);
            }
        }

        public void GetForeignKeys(ForeignKeyReader reader, string database)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = _sqlGetForeignKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                using (var r = cmd.ExecuteReader())
                    reader(r, _serverIdCtx, database);
            }
        }

        public void GetUniqueKeys(UniqueKeyReader reader, string database)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = _sqlGetUniqueKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                using (var r = cmd.ExecuteReader())
                    reader(r, _serverIdCtx, database);
            }
        }

        public object GetLastInsertedPk()
        {
            var cmd = _conn.CreateCommand();
            cmd.CommandText = _sqlGetLastInsertedPk;
            return cmd.ExecuteScalar();
        }

        public void EnforceIntegrityCheck(bool active)
        {
            var cmd = _conn.CreateCommand();

            var p = cmd.CreateParameter();
            p.ParameterName = "@ACTIVE";
            p.Value = active;
            p.DbType = DbType.Boolean;
            cmd.Parameters.Add(p);

            cmd.CommandText = _sqlEnforceIntegrityCheck;
            cmd.ExecuteNonQuery();
        }

        public object[][] Select(IRowIdentifier ri)
        {
            List<object[]> rows = new List<object[]>();
            TableSchema schema = ri.GetTable();
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
            TableSchema schema = ti.GetTable();
            if (schema.ColumnsDefinition.Count() != row.Length)
                throw new Exception("The row doesn't correspond to schema!");

            using (var cmd = _conn.CreateCommand())
            {
                //Add params
                for (int i = 0; i < schema.ColumnsDefinition.Count(); i++)
                {
                    if (schema.ColumnsDefinition[i].IsAutoIncrement) continue;

                    var p = cmd.CreateParameter();
                    p.ParameterName = "@" + schema.ColumnsDefinition[i].Name;
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

        /// <summary>
        /// Détermine le DbType correspondant au type SQL sous les formes :
        /// smallint(5) unsigned, varchar(50), decimal(5,2), timestamp, enum('G','P','R').
        /// </summary>
        /// <param name="fullType">Type de la colonne SQL</param>
        /// <returns>Type DbType</returns>     
        public virtual void SqlTypeToDbType(string fullType, out DbType type, out string size)
        {
            fullType = fullType.ToLower();
            int startPosLength = fullType.IndexOf("(");
            int endPosLength = fullType.IndexOf(")");
            string[] values = fullType.Split(' ');
            string strType;
            string[] descriptorValues = null;
            //int length;
            //int precision;
            bool? signedness = null;
            type = DbType.Object;
            size = null;

            //S'il y a une description du type entre ()
            if (startPosLength > -1 && endPosLength > startPosLength)
            {
                size = fullType.Substring(startPosLength + 1, endPosLength - startPosLength - 1);
                descriptorValues = size.Split(',');
                strType = values[0].Substring(0, startPosLength);
            }
            else
            {
                strType = values[0];
            }

            if (values.Length > 1)
                signedness = values[1] == "signed";

            //Parse descriptior
            //switch (strType)
            //{
            //    case "enum":
            //    case "set":
            //        type = DbType.Object; //Not supported
            //        break; 
            //    default:
            //        if (descriptorValues != null)
            //        {
            //            if (descriptorValues.Length > 1)
            //                precision = Int32.Parse(descriptorValues[1]);
            //            length = Int32.Parse(descriptorValues[0]);
            //        }
            //        break;
            //}

            //From unsigned to CLR data type
            if (signedness.HasValue && !signedness.Value)
            {
                switch (strType)
                {
                    case "tinyint":
                    case "smallint":
                    case "mediumint": //À vérifier
                        type = DbType.Int32;
                        break;
                    case "int":
                        type = DbType.Int64;
                        break;
                    case "bigint":
                        type = DbType.Decimal;
                        break;
                }
            }
            else
            {

                //From signed to CLR data type
                switch (strType)
                {
                    case "tinyint":
                        type = DbType.Byte;
                        break;
                    case "smallint":
                    case "year":
                        type = DbType.Int16;
                        break;
                    case "mediumint":
                    case "int":
                        type = DbType.Int32;
                        break;
                    case "bigint":
                    case "bit":
                        type = DbType.Int64;
                        break;
                    case "float":
                        type = DbType.Single;
                        break;
                    case "double":
                        type = DbType.Double;
                        break;
                    case "decimal":
                        type = DbType.Decimal;
                        break;
                    case "char":
                    case "varchar":
                    case "tinytext":
                    case "text":
                    case "mediumtext":
                    case "longtext":
                    case "binary":
                    case "varbinary":
                        type = DbType.String;
                        break;
                    case "tinyblob":
                    case "blob":
                    case "mediumblob":
                    case "longblob":
                    case "enum":
                    case "set":
                        type = DbType.Binary;
                        break;
                    case "date":
                    case "datetime":
                        type = DbType.DateTime;
                        break;
                    case "time":
                    case "timestamp":
                        type = DbType.Time;
                        break;
                }
            }
        }
    }
}
