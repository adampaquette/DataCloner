using DataCloner.Data.Generator;
using DataCloner.Framework;
using DataCloner.Internal;
using DataCloner.Metadata;
using DataCloner.PlugIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace DataCloner.Data
{
    internal abstract class QueryHelperBase : IQueryHelper
    {
        private readonly MetadataPerServer _metadata;
        private readonly IDbConnection _connection;

        protected abstract string SqlGetDatabasesName { get; }
        protected abstract string SqlGetColumns { get; }
        protected abstract string SqlGetForeignKeys { get; }
        protected abstract string SqlGetUniqueKeys { get; }
        protected abstract string SqlGetLastInsertedPk { get; }
        protected abstract string SqlEnforceIntegrityCheck { get; }

        public event QueryCommitingEventHandler QueryCommmiting;
        public IDbConnection Connection { get{return _connection;} }
        public abstract DbEngine Engine { get; }
        public abstract ISqlTypeConverter TypeConverter { get; }
        public abstract ISqlWriter SqlWriter { get;}

        protected QueryHelperBase(MetadataPerServer metadata, string providerName, string connectionString)
        {
            var factory = DbProviderFactories.GetFactory(providerName);
            _metadata = metadata;
            _connection = factory.CreateConnection();
            _connection.ConnectionString = connectionString;
        }

        public string[] GetDatabasesName()
        {
            var databases = new List<string>();

            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SqlGetDatabasesName;
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

        public void GetColumns(ColumnReader reader, Int16 serverId, string database)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SqlGetColumns;

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                Connection.Open();
                using (var r = cmd.ExecuteReader())
                    reader(r, serverId, database, TypeConverter);
                Connection.Close();
            }
        }

        public void GetForeignKeys(ForeignKeyReader reader, Int16 serverId, string database)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SqlGetForeignKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                Connection.Open();
                using (var r = cmd.ExecuteReader())
                    reader(r, serverId, database);
                Connection.Close();
            }
        }

        public void GetUniqueKeys(UniqueKeyReader reader, Int16 serverId, string database)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SqlGetUniqueKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = "@DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                Connection.Open();
                using (var r = cmd.ExecuteReader())
                    reader(r, serverId, database);
                Connection.Close();
            }
        }

        public object GetLastInsertedPk()
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = SqlGetLastInsertedPk;

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

            cmd.CommandText = SqlEnforceIntegrityCheck;

            Connection.Open();
            cmd.ExecuteNonQuery();
            Connection.Close();
        }

        public object[][] Select(IRowIdentifier row)
        {
            var rows = new List<object[]>();
            var tableMetadata = _metadata.GetTable(row);
            var query = new StringBuilder(tableMetadata.SelectCommand);
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
                    TryExecute(plan, query, transaction, ref cmd, ref nbParams, ref cancel);
                }

                foreach (var step in plan.UpdateSteps)
                {
                    GemerateUpdateStatment(step, cmd, query);
                    TryExecute(plan, query, transaction, ref cmd, ref nbParams, ref cancel);
                }

                //Flush
                Execute(plan, query, transaction, cmd, ref cancel);

                if (!cancel)
                    transaction.Commit();
            }
            Connection.Close();
        }

        private void TryExecute(ExecutionPlan plan, StringBuilder query, IDbTransaction transaction, ref IDbCommand cmd, 
                                ref int nbParams, ref bool cancel)
        {
            const int maxBatchSizeKo = 65536;
            const int maxParam = 1900;
            if (nbParams <= maxParam && query.Length <= maxBatchSizeKo) return;

            Execute(plan, query, transaction, cmd, ref cancel);

            nbParams = 0;
            query.Clear();
            cmd = Connection.CreateCommand();
        }

        private void Execute(ExecutionPlan plan, StringBuilder query, IDbTransaction transaction,
                             IDbCommand cmd, ref bool cancel)
        {
            if (query.Length <= 0) return;

            //We start a new batch
            cmd.CommandText = query.ToString();
            cmd.Transaction = transaction;

            //Exec query
            using (var dr = cmd.ExecuteReader())
                RetriveAutoGeneratedPk(plan, dr);

            RetriveOutputParams(plan, cmd);

            if (QueryCommmiting != null)
            {
                var args = new QueryCommitingEventArgs(cmd.GetGeneratedQuery());
                QueryCommmiting(null, args);
                if (args.Cancel) cancel = true;
            }
        }

        private void RetriveOutputParams(ExecutionPlan plan, IDbCommand cmd)
        {
            foreach (var parameter in cmd.Parameters)
            {
                var param = parameter as IDataParameter;
                if (param.Direction == ParameterDirection.Output)
                {
                    var paramId = Int32.Parse(param.ParameterName.Split('@')[1]);

                    //On récupère la valeur générée par SQL pour traitements futurs
                    foreach (var sqlVar in plan.Variables)
                    {
                        if (sqlVar.Id == paramId)
                            sqlVar.Value = param.Value;
                    }
                }
            }
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
            var tableMetadata = _metadata.GetTable(step.DestinationTable);
            if (tableMetadata.ColumnsDefinition.Count() != step.DataRow.Length)
                throw new Exception("The step doesn't correspond to schema!");

            var insertWriter = SqlWriter.GetInsertWriter()
                               .AppendColumns(step.DestinationTable, tableMetadata.ColumnsDefinition);

            var sbPostInsert = new StringBuilder();

            //Valeurs des colonnes
            for (var i = 0; i < tableMetadata.ColumnsDefinition.Count(); i++)
            {
                var col = tableMetadata.ColumnsDefinition[i];
                var sqlVar = step.DataRow[i] as SqlVariable;

                //Variable à générer
                if (((col.IsPrimary && !col.IsAutoIncrement) || col.IsUniqueKey) && !col.IsForeignKey)
                {
                    if (sqlVar == null) throw new NullReferenceException();

                    sqlVar.Value = DataBuilder.BuildDataColumn(this, step.DestinationTable.ServerId, step.DestinationTable.Database,
                                                               step.DestinationTable.Schema, step.TableSchema, col);
                    insertWriter.AppendValue(sqlVar.Value);
                }
                //Post insert variable (auto generated primary key)
                else if (col.IsPrimary && col.IsAutoIncrement)
                {
                    if (sqlVar == null) throw new NullReferenceException();

                    var sqlVarName = "@" + sqlVar.Id;

                    if (step.Depth == 0 || sqlVar.QueryValue)
                    {
                        //sbPostInsert.Append("DECLARE ").Append(sqlVarName).Append(" varchar(max);\r\n");
                        sbPostInsert.Append("SET ").Append(sqlVarName).Append(" = SCOPE_IDENTITY();\r\n");

                        //if (step.Depth == 0)
                        //sbPostInsert.Append("SELECT ").Append(sqlVar.Id).Append(" K, ").Append(sqlVarName).Append(" V;\r\n");
                        var p = cmd.CreateParameter();
                        p.ParameterName = sqlVarName;
                        p.DbType = col.DbType;
                        p.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(p);
                    }
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
                        var sqlVarName = "@" + tableMetadata.ColumnsDefinition[i].Name.FormatSqlParam() + step.StepId;
                        var p = cmd.CreateParameter();
                        p.ParameterName = sqlVarName;
                        p.Value = step.DataRow[i];
                        p.DbType = col.DbType;
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
    }
}
