using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.Generator;
using DataCloner.PlugIn;

namespace DataCloner.DataAccess
{
    internal abstract class AbstractQueryHelper : IQueryHelper
    {
        private readonly Cache _cache;
        private readonly Int16 _serverIdCtx;

        private readonly string _sqlGetDatabasesName;
        private readonly string _sqlGetColumns;
        private readonly string _sqlGetForeignKeys;
        private readonly string _sqlGetUniqueKeys;
        private readonly string _sqlGetLastInsertedPk;
        private readonly string _sqlEnforceIntegrityCheck;

        public event QueryCommitingEventHandler QueryCommmiting;

        public AbstractQueryHelper(Cache cache, string providerName, string connectionString, Int16 serverId,
            string sqlGetDatabasesName, string sqlGetColumns, string sqlGetForeignKeys, string sqlGetUniqueKeys,
            string sqlGetLastInsertedPk, string sqlEnforceIntegrityCheck)
        {
            var factory = DbProviderFactories.GetFactory(providerName);
            _cache = cache;
            Connection = factory.CreateConnection();
            Connection.ConnectionString = connectionString;

            _serverIdCtx = serverId;

            _sqlGetDatabasesName = sqlGetDatabasesName;
            _sqlGetColumns = sqlGetColumns;
            _sqlGetForeignKeys = sqlGetForeignKeys;
            _sqlGetUniqueKeys = sqlGetUniqueKeys;
            _sqlGetLastInsertedPk = sqlGetLastInsertedPk;
            _sqlEnforceIntegrityCheck = sqlEnforceIntegrityCheck;
        }

        public IDbConnection Connection { get; }

        public DbEngine Engine => DbEngine.MySql;

        public abstract ISqlTypeConverter TypeConverter { get; }
        public abstract ISqlWriter SqlWriter { get; }

        public string[] GetDatabasesName()
        {
            var databases = new List<string>();

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = _sqlGetDatabasesName;
                Connection.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        databases.Add(r.GetString(0).ToLower());
                }
                Connection.Close();
            }
            return databases.ToArray();
        }

        public void GetColumns(ColumnReader reader, string database)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = _sqlGetColumns;

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                Connection.Open();
                using (var r = cmd.ExecuteReader())
                    reader(r, _serverIdCtx, database, TypeConverter);
                Connection.Close();
            }
        }

        public void GetForeignKeys(ForeignKeyReader reader, string database)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = _sqlGetForeignKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                Connection.Open();
                using (var r = cmd.ExecuteReader())
                    reader(r, _serverIdCtx, database);
                Connection.Close();
            }
        }

        public void GetUniqueKeys(UniqueKeyReader reader, string database)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = _sqlGetUniqueKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                Connection.Open();
                using (var r = cmd.ExecuteReader())
                    reader(r, _serverIdCtx, database);
                Connection.Close();
            }
        }

        public object GetLastInsertedPk()
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = _sqlGetLastInsertedPk;

            Connection.Open();
            var result = cmd.ExecuteScalar();
            Connection.Close();
            return result;
        }

        public void EnforceIntegrityCheck(bool active)
        {
            var cmd = Connection.CreateCommand();

            var p = cmd.CreateParameter();
            p.ParameterName = "@ACTIVE";
            p.Value = active;
            p.DbType = DbType.Boolean;
            cmd.Parameters.Add(p);

            cmd.CommandText = _sqlEnforceIntegrityCheck;

            Connection.Open();
            cmd.ExecuteNonQuery();
            Connection.Close();
        }

        public object[][] Select(IRowIdentifier row)
        {
            var rows = new List<object[]>();
            var schema = _cache.GetTable(row);
            var query = new StringBuilder(schema.SelectCommand);
            var nbParams = row.Columns.Count;

            using (var cmd = Connection.CreateCommand())
            {
                //Build query / params
                if (nbParams > 0)
                    query.Append(" WHERE ");

                for (var i = 0; i < nbParams; i++)
                {
                    var paramName = row.Columns.ElementAt(i).Key;
                    query.Append(paramName).Append(" = @").Append(paramName);

                    var p = cmd.CreateParameter();
                    p.ParameterName = "@" + paramName;
                    p.Value = row.Columns.ElementAt(i).Value;
                    cmd.Parameters.Add(p);

                    if (i < nbParams - 1)
                        query.Append(" AND ");
                }
                cmd.CommandText = query.ToString();

                //Exec query
                Connection.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var values = new object[r.FieldCount];
                        r.GetValues(values);
                        rows.Add(values);
                    }
                }
                Connection.Close();
            }
            return rows.ToArray();
        }

        //public void Insert(ITableIdentifier table, object[] row)
        //{
        //	var schema = _cache.GetTable(table);
        //	if (schema.ColumnsDefinition.Count() != row.Length)
        //		throw new Exception("The step doesn't correspond to schema!");

        //	using (var cmd = Connection.CreateCommand())
        //	{
        //		//Add params
        //		for (var i = 0; i < schema.ColumnsDefinition.Count(); i++)
        //		{
        //			if (schema.ColumnsDefinition[i].IsAutoIncrement) continue;

        //			var p = cmd.CreateParameter();
        //			p.ParameterName = "@" + schema.ColumnsDefinition[i].Name;
        //			p.Value = row[i];
        //			cmd.Parameters.Add(p);
        //		}
        //		cmd.CommandText = schema.InsertCommand;

        //		//Exec query
        //		Connection.Open();
        //		using (var transaction = Connection.BeginTransaction())
        //		{
        //			cmd.ExecuteNonQuery();
        //			transaction.Commit();
        //		}
        //		Connection.Close();
        //	}
        //}

        public void Execute(ExecutionPlan plan)
        {
            var query = new StringBuilder();           
            var cmd = Connection.CreateCommand();
            var nbParams = 0;

            Connection.Open();
            using (var transaction = Connection.BeginTransaction())
            {
                var cancel = false;

                foreach (var step in plan.InsertSteps)
                {
                    GemerateInsertStatment(step, cmd, query, ref nbParams);
                    TryExecute(plan, query, transaction, ref nbParams, ref cmd, ref cancel);
                }

                foreach (var step in plan.UpdateSteps)
                {
                    GemerateUpdateStatment(step, cmd, query);
                    TryExecute(plan, query, transaction, ref nbParams, ref cmd, ref cancel);
                }

                if (!cancel)
                    transaction.Commit();
            }
            Connection.Close();
        }

        private void TryExecute(ExecutionPlan plan, StringBuilder query, IDbTransaction transaction, 
                                ref int nbParams, ref IDbCommand cmd, ref bool cancel)
        {
            var maxBatchSizeKo = 65536;
            var maxParam = 1900;

            //We start a new batch
            if (nbParams > maxParam || query.Length > maxBatchSizeKo)
            {
                cmd.CommandText = query.ToString();
                cmd.Transaction = transaction;

                //Exec query
                using (var dr = cmd.ExecuteReader())
                    RetriveAutoGeneratedPk(plan, dr);

                if (QueryCommmiting != null)
                {
                    var args = new QueryCommitingEventArgs(GetGeneratedQuery(cmd));
                    QueryCommmiting(null, args);
                    if (args.Cancel) cancel = true;
                }

                nbParams = 0;
                query.Clear();
                cmd = Connection.CreateCommand();
            }
        }

        //TODO : CLASSE À PART EXTENSION STATIQUE
        public string GetGeneratedQuery(IDbCommand dbCommand)
        {
            var query = dbCommand.CommandText;
            foreach (var parameter in dbCommand.Parameters)
            {
                var param = parameter as MySql.Data.MySqlClient.MySqlParameter;
                query = query.Replace(param.ParameterName, param.Value.ToString().EscapeSql());
            }

            return query;
        }

        private static void RetriveAutoGeneratedPk(ExecutionPlan plan, IDataReader dr)
        {
            //Pour chaque requête
            do
            {
                //Pour chaque ligne
                while (dr.Read())
                {
                    var stepId = dr.GetInt32(0);
                    var value = dr.GetValue(1);

                    //On récupère la valeur générée par SQL pour traitements futurs
                    foreach (var sqlVar in plan.Variables)
                    {
                        if (sqlVar.Id == stepId)
                            sqlVar.Value = value;
                    }
                }
            } while (dr.NextResult());
        }

        internal void GemerateInsertStatment(InsertStep step, IDbCommand cmd, StringBuilder sql, ref int nbParams)
        {
            var schema = _cache.GetTable(step.DestinationTable);
            if (schema.ColumnsDefinition.Count() != step.DataRow.Length)
                throw new Exception("The step doesn't correspond to schema!");

            var insertWriter = SqlWriter.GetInsertWriter()
                               .AppendColumns(step.DestinationTable, schema.ColumnsDefinition);

            var sbPostInsert = new StringBuilder();

            //Valeurs des colonnes
            for (var i = 0; i < schema.ColumnsDefinition.Count(); i++)
            {
                var col = schema.ColumnsDefinition[i];
                var sqlVar = step.DataRow[i] as SqlVariable;

                //Variable à générer
                if (((col.IsPrimary && !col.IsAutoIncrement) || col.IsUniqueKey) && !col.IsForeignKey)
                {
                    sqlVar.Value = DataBuilder.BuildDataColumn(this, step.DestinationTable.ServerId, step.DestinationTable.Database,
                                                               step.DestinationTable.Schema, step.TableSchema, col);
                    insertWriter.AppendValue(sqlVar.Value);
                }
                //Post insert variable (auto generated primary key)
                else if (col.IsPrimary && col.IsAutoIncrement)
                {
                    var sqlVarName = "@" + sqlVar.Id;

                    if (sqlVar.QueryValue)
                    {
                        sbPostInsert.Append("DECLARE ").Append(sqlVarName).Append(" varchar(max);\r\n");
                        sbPostInsert.Append("SET ").Append(sqlVarName).Append(" = SCOPE_IDENTITY();\r\n");
                    }

                    //Get the generated id only for top level query
                    if (step.Depth == 0)
                        sbPostInsert.Append("SELECT ").Append(sqlVar.Id).Append(" K, ").Append(sqlVarName).Append(" V;\r\n");
                }
                //Normal variables
                else
                {
                    //On fait référence à une variable
                    if (sqlVar != null)
                    {
                        //Si variable auto-généré
                        if (sqlVar.Value == null)
                            insertWriter.AppendVariable(sqlVar.Id.ToString());
                        //Sinon variable déjà générée
                        else
                            insertWriter.AppendValue(sqlVar.Value);
                    }
                    //C'est une valeur brute
                    else
                    {
                        var sqlVarName = "@" + schema.ColumnsDefinition[i].Name.FormatSqlParam() + step.StepId;
                        var p = cmd.CreateParameter();
                        p.ParameterName = sqlVarName;
                        p.Value = step.DataRow[i];
                        cmd.Parameters.Add(p);

                        insertWriter.Append(sqlVarName).Append(",");
                        nbParams++;
                    }
                }
            }
            insertWriter.Complete();

            sql.Append(insertWriter.ToStringBuilder())
               .Append(sbPostInsert);
        }

        public void GemerateUpdateStatment(UpdateStep step, IDbCommand cmd, StringBuilder sql)
        {
            if (!step.DestinationRow.Columns.Any())
                throw new ArgumentNullException("You must specify at least one column in the step identifier.");

            sql.Append("UPDATE ")
               .Append(step.DestinationRow.Database)
               .Append(".")
               .Append(step.DestinationRow.Table)
               .Append(" SET ");

            foreach (var col in step.ForeignKey)
            {
                var paramName = col.Key.FormatSqlParam();

                sql.Append('"').Append(col.Key).Append('"')
                   .Append(" = @")
                   .Append(paramName)
                   .Append(step.StepId)
                   .Append(",");

                var p = cmd.CreateParameter();
                p.ParameterName = "@" + paramName + step.StepId;

                var sqlVar = col.Value as SqlVariable;
                p.Value = sqlVar.Value ?? "@" + sqlVar.Id;

                cmd.Parameters.Add(p);
            }
            sql.Remove(sql.Length - 1, 1);
            sql.Append(" WHERE ");

            foreach (var kv in step.DestinationRow.Columns)
            {
                var paramName = kv.Key.FormatSqlParam();

                sql.Append('"').Append(kv.Key).Append('"')
                   .Append(" = @")
                   .Append(paramName)
                   .Append(step.StepId)
                   .Append(" AND ");

                var p = cmd.CreateParameter();
                p.ParameterName = "@" + paramName + step.StepId;

                var sqlVar = kv.Value as SqlVariable;
                p.Value = sqlVar.Value ?? "@" + sqlVar.Id;

                cmd.Parameters.Add(p);
            }
            sql.Remove(sql.Length - 5, 5);
            sql.Append(";\r\n");
        }

        //public void Update(IRowIdentifier row, ColumnsWithValue values)
        //{
        //	if (!row.Columns.Any())
        //		throw new ArgumentNullException("You must specify at least one column in the step identifier.");

        //	var cmd = Connection.CreateCommand();
        //	var sql = new StringBuilder("UPDATE ");
        //	sql.Append(row.Database)
        //	   .Append(".")
        //	   .Append(row.Table)
        //	   .Append(" SET ");

        //	foreach (var col in values)
        //	{
        //		var paramName = col.Key.FormatSqlParam();

        //		sql.Append('"').Append(col.Key).Append('"')
        //		   .Append(" = @")
        //		   .Append(paramName)
        //		   .Append(",");

        //		var p = cmd.CreateParameter();
        //		p.ParameterName = "@" + paramName;
        //		p.Value = col.Value;
        //		cmd.Parameters.Add(p);
        //	}
        //	sql.Remove(sql.Length - 1, 1);
        //	sql.Append(" WHERE ");

        //	foreach (var kv in row.Columns)
        //	{
        //		var paramName = kv.Key.FormatSqlParam();

        //		sql.Append('"').Append(kv.Key).Append('"')
        //		   .Append(" = @")
        //		   .Append(paramName)
        //		   .Append(" AND ");

        //		var p = cmd.CreateParameter();
        //		p.ParameterName = "@" + paramName;
        //		p.Value = kv.Value;
        //		cmd.Parameters.Add(p);
        //	}
        //	sql.Remove(sql.Length - 5, 5);

        //	cmd.CommandText = sql.ToString();

        //	Connection.Open();
        //	using (var transaction = Connection.BeginTransaction())
        //	{
        //		cmd.ExecuteNonQuery();
        //		transaction.Commit();
        //	}
        //	Connection.Close();
        //}

        //public void Delete(IRowIdentifier row)
        //{
        //	var cmd = Connection.CreateCommand();
        //	var sql = new StringBuilder("DELETE FROM ");
        //	sql.Append(row.Database)
        //	   .Append(".")
        //	   .Append(row.Table);

        //	if (row.Columns.Count > 0)
        //		sql.Append(" WHERE 1=1");

        //	foreach (var kv in row.Columns)
        //	{
        //		var paramName = kv.Key.FormatSqlParam();

        //		sql.Append(" AND ")
        //		   .Append('"').Append(kv.Key).Append('"')
        //		   .Append(" = @")
        //		   .Append(paramName);

        //		var p = cmd.CreateParameter();
        //		p.ParameterName = "@" + paramName;
        //		p.Value = kv.Value;
        //		cmd.Parameters.Add(p);
        //	}

        //	cmd.CommandText = sql.ToString();

        //	Connection.Open();
        //	using (var transaction = Connection.BeginTransaction())
        //	{
        //		cmd.ExecuteNonQuery();
        //		transaction.Commit();
        //	}
        //	Connection.Close();
        //}

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
        //public virtual void SqlTypeToDbType(string fullType, out DbType type, out string size)
        //{
        //	fullType = fullType.ToLower();
        //	var startPosLength = fullType.IndexOf("(", StringComparison.Ordinal);
        //	var endPosLength = fullType.IndexOf(")", StringComparison.Ordinal);
        //	var values = fullType.Split(' ');
        //	string strType;
        //	string[] descriptorValues = null;
        //	//int length;
        //	//int precision;
        //	bool? signedness = null;
        //	type = DbType.Object;
        //	size = null;

        //	//S'il y a une description du type entre ()
        //	if (startPosLength > -1 && endPosLength > startPosLength)
        //	{
        //		size = fullType.Substring(startPosLength + 1, endPosLength - startPosLength - 1);
        //		descriptorValues = size.Split(',');
        //		strType = values[0].Substring(0, startPosLength);
        //	}
        //	else
        //	{
        //		strType = values[0];
        //	}

        //	if (values.Length > 1)
        //		signedness = values[1] == "signed";

        //	//Parse descriptior
        //	//switch (strType)
        //	//{
        //	//    case "enum":
        //	//    case "set":
        //	//        type = DbType.Object; //Not supported
        //	//        break; 
        //	//    default:
        //	//        if (descriptorValues != null)
        //	//        {
        //	//            if (descriptorValues.Length > 1)
        //	//                precision = Int32.Parse(descriptorValues[1]);
        //	//            length = Int32.Parse(descriptorValues[0]);
        //	//        }
        //	//        break;
        //	//}

        //	//From unsigned to CLR data type
        //	if (signedness.HasValue && !signedness.Value)
        //	{
        //		switch (strType)
        //		{
        //			case "tinyint":
        //			case "smallint":
        //			case "mediumint": //À vérifier
        //				type = DbType.Int32;
        //				break;
        //			case "int":
        //				type = DbType.Int64;
        //				break;
        //			case "bigint":
        //				type = DbType.Decimal;
        //				break;
        //		}
        //	}
        //	else
        //	{
        //		//From signed to CLR data type
        //		switch (strType)
        //		{
        //			case "tinyint":
        //				type = DbType.Byte;
        //				break;
        //			case "smallint":
        //			case "year":
        //				type = DbType.Int16;
        //				break;
        //			case "mediumint":
        //			case "int":
        //				type = DbType.Int32;
        //				break;
        //			case "bigint":
        //			case "bit":
        //				type = DbType.Int64;
        //				break;
        //			case "float":
        //				type = DbType.Single;
        //				break;
        //			case "double":
        //				type = DbType.Double;
        //				break;
        //			case "decimal":
        //				type = DbType.Decimal;
        //				break;
        //			case "char":
        //			case "varchar":
        //			case "tinytext":
        //			case "text":
        //			case "mediumtext":
        //			case "longtext":
        //			case "binary":
        //			case "varbinary":
        //				type = DbType.String;
        //				break;
        //			case "tinyblob":
        //			case "blob":
        //			case "mediumblob":
        //			case "longblob":
        //			case "enum":
        //			case "set":
        //				type = DbType.Binary;
        //				break;
        //			case "date":
        //			case "datetime":
        //				type = DbType.DateTime;
        //				break;
        //			case "time":
        //			case "timestamp":
        //				type = DbType.Time;
        //				break;
        //		}
        //	}
        //}
    }

    internal static class SqlExtensions
    {
        internal static string FormatSqlParam(this string value)
        {
            return value.Replace(" ", String.Empty);
        }

        internal static string EscapeSql(this string value)
        {
            return value.Replace("'", "''");
        }
    }
}
