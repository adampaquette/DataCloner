using DataCloner.Core.Data.Generator;
using DataCloner.Core.Framework;
using DataCloner.Core.Internal;
using DataCloner.Core.Metadata;
using DataCloner.Core.PlugIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DataCloner.Core.Metadata.Context;

namespace DataCloner.Core.Data
{
    internal abstract class QueryHelperBase : IQueryHelper
    {
        private readonly Metadatas _metadata;
        private readonly IDbConnection _connection;

        /// <summary>
        /// SQL query
        /// </summary>
        protected abstract string SqlGetDatabasesName { get; }
        /// <summary>
        /// SQL query
        /// </summary>
        protected abstract string SqlGetColumns { get; }
        /// <summary>
        /// SQL query
        /// </summary>
        protected abstract string SqlGetForeignKeys { get; }
        /// <summary>
        /// SQL query
        /// </summary>
        protected abstract string SqlGetUniqueKeys { get; }
        /// <summary>
        /// SQL query
        /// </summary>
        protected abstract string SqlGetLastInsertedPk { get; }
        /// <summary>
        /// SQL query
        /// </summary>
        protected abstract string SqlEnforceIntegrityCheck { get; }
        
        public event QueryCommitingEventHandler QueryCommmiting;
        public IDbConnection Connection { get; }
        public abstract DbEngine Engine { get; }
        public abstract ISqlTypeConverter TypeConverter { get; }
        public abstract ISqlWriter SqlWriter { get;}

        protected QueryHelperBase(Metadatas metadata, string providerName, string connectionString)
        {
            DbProviderFactory factory;
            //var factory = DbProviderFactories.GetFactory(providerName);
            switch (providerName)
            {
                case QueryHelperMsSql.ProviderName:
                    factory = SqlClientFactory.Instance;
                    break;
                default:
                    throw new Exception("Provider not supported");
            }

            _metadata = metadata;
            Connection = factory.CreateConnection();
            Connection.ConnectionString = connectionString;
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
                        databases.Add(r.GetString(0));
                }
                Connection.Close();
            }
            return databases.ToArray();
        }

        public void GetColumns(ColumnReader reader, Metadatas metadata, Int16 serverId, string database)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SqlGetColumns;

                var p = cmd.CreateParameter();
                p.ParameterName = SqlWriter.NamedParamPrefix + "DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                Connection.Open();
                using (var r = cmd.ExecuteReader())
                    reader(r, metadata, serverId, database, TypeConverter);
                Connection.Close();
            }
        }

        public void GetForeignKeys(ForeignKeyReader reader, Metadatas metadata, Int16 serverId, string database)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SqlGetForeignKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = SqlWriter.NamedParamPrefix + "DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                Connection.Open();
                using (var r = cmd.ExecuteReader())
                    reader(r, metadata, serverId, database);
                Connection.Close();
            }
        }

        public void GetUniqueKeys(UniqueKeyReader reader, Metadatas metadata, Int16 serverId, string database)
        {
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = SqlGetUniqueKeys;

                var p = cmd.CreateParameter();
                p.ParameterName = SqlWriter.NamedParamPrefix + "DATABASE";
                p.Value = database;
                cmd.Parameters.Add(p);

                Connection.Open();
                using (var r = cmd.ExecuteReader())
                    reader(r, metadata, serverId, database);
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
            p.ParameterName = SqlWriter.NamedParamPrefix + "ACTIVE";
            p.Value = active;
            p.DbType = DbType.Boolean;
            cmd.Parameters.Add(p);

            cmd.CommandText = SqlEnforceIntegrityCheck;

            Connection.Open();
            cmd.ExecuteNonQuery();
            Connection.Close();
        }

        public object[][] Select(RowIdentifier row)
        {
            var rows = new List<object[]>();
            var tableMetadata = _metadata.GetTable(row);           
            var nbParams = row.Columns.Count;
            var selectWriter = SqlWriter.GetSelectWriter()
                                        .AppendColumns(row, tableMetadata.ColumnsDefinition);

            using (var cmd = Connection.CreateCommand())
            {
                //Build query / params
                for (var i = 0; i < nbParams; i++)
                {
                    var colName = row.Columns.ElementAt(i).Key;
                    var paramName = SqlWriter.NamedParamPrefix + colName;

                    var p = cmd.CreateParameter();
                    p.ParameterName = paramName;
                    p.Value = row.Columns.ElementAt(i).Value;
                    cmd.Parameters.Add(p);

                    selectWriter.AppendToWhere(colName, paramName);
                }
                cmd.CommandText = selectWriter.ToStringBuilder().ToString();

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
                    GemerateInsertStatment(step, cmd, query, transaction, ref nbParams);
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

            if (QueryCommmiting != null)
            {
                var args = new QueryCommitingEventArgs(cmd);
                QueryCommmiting(null, args);
                if (args.Cancel) cancel = true;
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

        internal void GemerateInsertStatment(InsertStep step, IDbCommand cmd, StringBuilder sql, IDbTransaction transaction, ref int nbParams)
        {
            var tableMetadata = _metadata.GetTable(step.DestinationTable);
            if (tableMetadata.ColumnsDefinition.Count() != step.Datarow.Length)
                throw new Exception("The step doesn't correspond to schema!");

            var insertWriter = SqlWriter.GetInsertWriter()
                               .AppendColumns(step.DestinationTable, tableMetadata.ColumnsDefinition);

            var sbPostInsert = new StringBuilder();

            //Valeurs des colonnes
            for (var i = 0; i < tableMetadata.ColumnsDefinition.Count(); i++)
            {
                var col = tableMetadata.ColumnsDefinition[i];
                var sqlVar = step.Datarow[i] as SqlVariable;

                //Variable à générer
                if (((col.IsPrimary && !col.IsAutoIncrement) || col.IsUniqueKey) && !col.IsForeignKey)
                {
                    if (sqlVar == null) throw new NullReferenceException();

                    sqlVar.Value = DataBuilder.BuildDataColumn(this, transaction, step.DestinationTable.ServerId, step.DestinationTable.Database,
                                                               step.DestinationTable.Schema, step.TableSchema, col);
                    insertWriter.AppendValue(sqlVar.Value);
                }
                //Post insert variable (auto generated primary key)
                else if (col.IsPrimary && col.IsAutoIncrement)
                {
                    if (sqlVar == null) throw new NullReferenceException();

                    if (step.Depth == 0 || sqlVar.QueryValue)
                        sbPostInsert.Append(SqlWriter.SelectLastIdentity(sqlVar.Id, tableMetadata.Name, col.Name));
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
                        var sqlVarName = SqlWriter.NamedParamPrefix + tableMetadata.ColumnsDefinition[i].Name.FormatSqlParam() + step.StepId;
                        var p = cmd.CreateParameter();
                        p.ParameterName = sqlVarName;

                        if(col.IsDataColumnBuildable())
                            p.Value = DataBuilder.BuildDataColumn(this, transaction, step.DestinationTable.ServerId, step.DestinationTable.Database,
                                                                  step.DestinationTable.Schema, step.TableSchema, col);
                        else
                            p.Value = step.Datarow[i];
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

        /// <summary>
        /// Update previously nulled foreign keys with a circular reference problem to the newly created one.
        /// We persist the problem to assure the data integrity.
        /// </summary>
        /// <param name="step">Step</param>
        /// <param name="cmd">Command</param>
        /// <param name="sql">Sql query</param>
        public void GemerateUpdateStatment(UpdateStep step, IDbCommand cmd, StringBuilder sql)
        {
            if (!step.DestinationRow.Columns.Any())
                throw new ArgumentNullException("You must specify at least one column in the step identifier.");

            var updateWriter = SqlWriter.GetUpdateWriter(step);

            foreach (var col in step.ForeignKey)
            {
                var paramName = SqlWriter.NamedParamPrefix + col.Key.FormatSqlParam() + step.StepId;
                var sqlVar = col.Value as SqlVariable;

                var p = cmd.CreateParameter();
                p.ParameterName =  paramName;
                p.Value = sqlVar.Value ?? SqlWriter.NamedParamPrefix + sqlVar.Id;

                cmd.Parameters.Add(p);

                updateWriter.AppendToSet(col.Key, paramName);
            }

            foreach (var kv in step.DestinationRow.Columns)
            {
                var paramName = SqlWriter.NamedParamPrefix + kv.Key.FormatSqlParam() + step.StepId;
                var sqlVar = kv.Value as SqlVariable;

                var p = cmd.CreateParameter(); 
                p.ParameterName =  paramName;
                p.Value = sqlVar.Value ?? SqlWriter.NamedParamPrefix + sqlVar.Id;

                cmd.Parameters.Add(p);

                updateWriter.AppendToWhere(kv.Key, paramName);
            }

            sql.Append(updateWriter.ToStringBuilder());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connection != null)
                {
                    if (Connection.State != ConnectionState.Closed)
                        Connection.Close();
                    Connection.Dispose();
                }
            }
        }
    }
}
