using DataCloner.Core.Framework;
using DataCloner.Core.Internal;
using DataCloner.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using DataCloner.Core.Configuration;
using DataCloner.Core.Metadata.Context;
using DataBuilder = DataCloner.Core.PlugIn.DataBuilder;
using DataCloner.Core.Clone;

namespace DataCloner.Core.Plan
{
    /// <summary>
    /// Class responsible for collecting data on multiples databases to create a blueprint of a desired object.
    /// It builds a <see cref="Query" that will then be executed on Sql Servers./>
    /// </summary>
    public class ExecutionPlanBuilder
    {
        private readonly ExecutionPlanByServer _executionPlanByServer;
        private readonly KeyRelationship _keyRelationships;
        private readonly List<CircularKeyJob> _circularKeyJobs;
        private readonly List<RowIdentifier> _steps;
        private int _nextVariableId;
        private int _nextStepId;

        private IExecutionContext ExecutionContext { get; }

        public Metadatas Metadatas => ExecutionContext.ConnectionsContext.Metadatas;
        public ConnectionsContext ConnectionsContext => ExecutionContext.ConnectionsContext;

        public event StatusChangedEventHandler StatusChanged;

        internal ExecutionPlanBuilder()
        {
            _keyRelationships = new KeyRelationship();
            _circularKeyJobs = new List<CircularKeyJob>();
            _executionPlanByServer = new ExecutionPlanByServer();
            _steps = new List<RowIdentifier>();
        }

        /// <summary>
        /// Constructor used for unit tests.
        /// </summary>
        /// <param name="project">User configuration influencing the execution plan.</param>
        /// <param name="cloningContext">Cloning task</param>
        public ExecutionPlanBuilder(Project project, CloningContext cloningContext) : this()
        {
            ExecutionContext = new ExecutionContext();
            ExecutionContext.Initialize(project, cloningContext);
        }

        /// <summary>
        /// Constructor used for unit tests.
        /// </summary>
        /// <param name="project">User configuration influencing the execution plan.</param>
        /// <param name="cloningContext">Cloning task</param>
        /// <param name="executionContext">ExecutionContext</param>
        internal ExecutionPlanBuilder(Project project, CloningContext cloningContext, ExecutionContext executionContext) : this()
        {
            if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

            ExecutionContext = executionContext;
            ExecutionContext.Initialize(project, cloningContext);
        }

        #region Public methods

        /// <summary>
        /// Append a blueprint of the desired object to the execution plan.
        /// </summary>
        /// <param name="sourceData">Source data representing 0 to N rows.</param>
        /// <param name="getDerivatives">If set to 1 the Sql data referencing the <see cref="sourceData"/> will be collected.</param>
        /// <returns>A reference to this <see cref="ExecutionPlanBuilder"/>.</returns>
        public ExecutionPlanBuilder Append(RowIdentifier sourceData, bool getDerivatives = true)
        {
            if (sourceData == null) throw new ArgumentNullException(nameof(sourceData));

            _steps.Add(sourceData);
            var rowsGenerating = new Stack<RowIdentifier>();
            rowsGenerating.Push(sourceData);

            BuildExecutionPlan(sourceData, getDerivatives, false, 0, rowsGenerating);
            BuildCircularReferencesPlan();

            return this;
        }

        /// <summary>
        /// Used for regenerating data. Lighten the memory footprint.
        /// </summary>
        public void Clear()
        {
            _executionPlanByServer.Clear();
            _keyRelationships.Clear();
            _circularKeyJobs.Clear();
            _steps.Clear();
            _nextVariableId = 0;
            _nextStepId = 0;
            DataBuilder.ClearBuildersCache();
        }

        /// <summary>
        /// Produce a <see cref="Query"/> that can be executed, saved and loaded endlessly.
        /// </summary>
        /// <returns>An executable query.</returns>
        public Query Compile()
        {
            OptimizeExecutionPlans(_executionPlanByServer);

            //Purify
            var conns = new List<Connection>();
            var metadata = new Metadatas();

            var destinationSrv = (from server in _executionPlanByServer
                                  from insertStep in server.Value.InsertSteps
                                  select insertStep.DestinationTable.ServerId).Distinct();

            foreach (var srv in destinationSrv)
            {
                conns.Add(ConnectionsContext.Connections.Keys.First(c => c.Id == srv));
                metadata.Add(srv, Metadatas.First(s => s.Key == srv).Value);
            }

            return new Query(ConnectionsContext, _executionPlanByServer, Query.CurrentFormatVersion);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Optimize the execution plan by removing unnescessary data.
        /// </summary>
        /// <param name="plans">Execution plan to optimize.</param>
        private static void OptimizeExecutionPlans(Dictionary<string, ExecutionPlan> plans)
        {
            var data = new List<object>();

            foreach (var plan in plans)
            {
                plan.Value.InsertSteps.ForEach(s => data.AddRange(s.Datarow));
                plan.Value.UpdateSteps.ForEach(s =>
                {
                    data.AddRange(s.DestinationRow.Columns.Values);
                    data.AddRange(s.ForeignKey.Values);
                });
            }

            var sqlVarsRefCount = new Dictionary<SqlVariable, int>();

            //We count the number of references
            foreach (var value in data)
            {
                var sqlVar = value as SqlVariable;
                if (sqlVar != null)
                {
                    if (!sqlVarsRefCount.ContainsKey(sqlVar))
                        sqlVarsRefCount.Add(sqlVar, 0);

                    sqlVarsRefCount[sqlVar]++;
                }
            }

            //We remove variables with les then 2 references
            foreach (var sqlVarRefCount in sqlVarsRefCount)
            {
                if (sqlVarRefCount.Value < 2)
                    sqlVarRefCount.Key.QueryValue = false;
            }

            //var memoryEntriesOptimized = sqlVarsRefCount.Count((sv) => sv.Value < 2); 
        }

        /// <summary>
        /// Build an execution plan for the specified data source for blazing fast performance.
        /// </summary>
        /// <param name="sourceRow">Identify a single or multiples rows to clone.</param>
        /// <param name="getDerivatives">When true the data related to the input(s) line(s) from other tables will be cloned.</param>
        /// <param name="shouldReturnFk">Indicate that a source row should only return a single line.</param>
        /// <param name="level">Current recursion level.</param>
        /// <param name="rowsGenerating">Current stack to handle circular foreign keys.</param>
        /// <returns>Always return the primary key of the source row, same if the value queried is a foreign key.</returns>
        private RowIdentifier BuildExecutionPlan(RowIdentifier sourceRow, bool getDerivatives, bool shouldReturnFk, int level,
                                                 Stack<RowIdentifier> rowsGenerating)
        {
            var srcRows = ConnectionsContext.Select(sourceRow);
            var nbRows = srcRows.Length;
            var table = Metadatas.GetTable(sourceRow);

            //By default the destination server is the source if no road is found.
            var serverDst = new SehemaIdentifier
            {
                ServerId = sourceRow.ServerId,
                Database = sourceRow.Database,
                Schema = sourceRow.Schema
            };

            if (ExecutionContext.Map.ContainsKey(serverDst))
                serverDst = ExecutionContext.Map[serverDst];

            var riReturn = new RowIdentifier
            {
                ServerId = serverDst.ServerId,
                Database = serverDst.Database,
                Schema = serverDst.Schema,
                Table = sourceRow.Table
            };
            var tiDestination = new TableIdentifier
            {
                ServerId = serverDst.ServerId,
                Database = serverDst.Database,
                Schema = serverDst.Schema,
                Table = sourceRow.Table
            };

            LogStatusChanged(sourceRow, level);

            if (shouldReturnFk && nbRows > 1)
                throw new Exception("The foreignkey is not unique!");

            //For each row
            for (var i = 0; i < nbRows; i++)
            {
                var currentRow = srcRows[i];
                var srcKey = table.BuildRawPkFromDataRow(currentRow);

                //Si ligne déjà enregistrée
                var dstKey = _keyRelationships.GetKey(serverDst.ServerId, serverDst.Database,
                                                      serverDst.Schema, sourceRow.Table, srcKey);
                if (dstKey != null)
                {
                    if (shouldReturnFk)
                    {
                        //Construit la pk de retour
                        riReturn.Columns = table.BuildPkFromRawKey(dstKey);
                        return riReturn;
                    }
                    continue;
                }
                var destinationRow = (object[])currentRow.Clone();
                foreach (var fk in table.ForeignKeys)
                {
                    //On skip si la FK est null
                    var fkValue = table.BuildRawFkFromDataRow(fk, currentRow);
                    if (fkValue.Contains(DBNull.Value))
                        continue;

                    //Si la foreignkey a déjà été enregistrée, on l'utilise
                    var fkDst = _keyRelationships.GetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, fkValue);
                    if (fkDst != null)
                        table.SetFkInDatarow(fk, fkDst, destinationRow);
                    else
                    {
                        var fkDestinationExists = false;
                        var fkTable = Metadatas.GetTable(fk);
                        var riFk = new RowIdentifier
                        {
                            ServerId = fk.ServerIdTo,
                            Database = fk.DatabaseTo,
                            Schema = fk.SchemaTo,
                            Table = fk.TableTo,
                            Columns = table.BuildKeyFromDerivativeDataRow(fk, currentRow)
                        };

                        //On ne duplique pas la ligne si la table est statique
                        if (fkTable.IsStatic)
                        {
                            //TODO : Tester si la FK existe dans la table de destination de clônage et non si la fk existe dans la bd source
                            var fkRow = ConnectionsContext.Select(riFk);
                            fkDestinationExists = fkRow.Length == 1;

                            if (fkRow.Length > 1)
                                throw new Exception("The FK is not unique.");

                            //Si la ligne existe déjà, on l'utilise
                            if (fkDestinationExists)
                            {
                                //Sauve la clef
                                var colFkObj = fkTable.BuildRawPkFromDataRow(fkRow[0]);
                                _keyRelationships.SetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, colFkObj, colFkObj);
                            }
                        }

                        //La FK n'existe pas, on la crer
                        if (!fkDestinationExists)
                        {
                            //Si référence circulaire
                            if (rowsGenerating.Contains(riFk))
                            {
                                //Affecte la FK à 1 pour les contraintes NOT NULL. EnforceIntegrity doit être désactivé.
                                var nullFk = Enumerable.Repeat<object>(1, fk.Columns.Count).ToArray();
                                table.SetFkInDatarow(fk, nullFk, destinationRow);

                                //On ajoute une tâche pour réassigner la FK "correctement", une fois que toute la chaîne aura été enregistrée.
                                _circularKeyJobs.Add(new CircularKeyJob
                                {
                                    SourceBaseRowStartPoint = new RowIdentifier
                                    {
                                        ServerId = sourceRow.ServerId,
                                        Database = sourceRow.Database,
                                        Schema = sourceRow.Schema,
                                        Table = sourceRow.Table,
                                        Columns = table.BuildPkFromDataRow(currentRow)
                                    },
                                    SourceFkRowStartPoint = riFk,
                                    ForeignKey = fk
                                });
                            }
                            else
                            {
                                //Crer la ligne et ses dépendances
                                rowsGenerating.Push(riFk);
                                var riNewFk = BuildExecutionPlan(riFk, false, true, level + 1, rowsGenerating);
                                rowsGenerating.Pop();

                                var newFkRow = GetDataRow(riNewFk);

                                //Affecte la clef
                                table.SetFkFromDatarowInDatarow(fkTable, fk, newFkRow, destinationRow);
                            }
                        }
                    }
                }

                var step = CreateExecutionStep(sourceRow, tiDestination, table, destinationRow, level);

                //Sauve la PK dans la cache
                dstKey = table.BuildRawPkFromDataRow(step.Datarow);
                _keyRelationships.SetKey(sourceRow.ServerId, sourceRow.Database, sourceRow.Schema, sourceRow.Table, srcKey, dstKey);

                //Ajouter les colonnes de contrainte unique dans _keyRelationships
                //...

                //On affecte la valeur de retour
                if (shouldReturnFk)
                {
                    riReturn.Columns = table.BuildPkFromRawKey(dstKey);
                }

                //On clone les lignes des tables dépendantes
                GetDerivatives(table, currentRow, getDerivatives, level, rowsGenerating);
            }

            return riReturn;
        }

        private void AddInsertStep(InsertStep step)
        {
            var connId = step.DestinationTable.ServerId;
            if (!_executionPlanByServer.ContainsKey(connId))
                _executionPlanByServer.Add(connId, new ExecutionPlan());
            _executionPlanByServer[connId].InsertSteps.Add(step);

            //Recopie dans le plan d'exécution pour la performance
            foreach (var sqlVar in step.Variables)
                _executionPlanByServer[connId].Variables.Add(sqlVar);
        }

        private void AddUpdateStep(UpdateStep step)
        {
            var connId = step.DestinationTable.ServerId;
            if (!_executionPlanByServer.ContainsKey(connId))
                _executionPlanByServer.Add(connId, new ExecutionPlan());
            _executionPlanByServer[connId].UpdateSteps.Add(step);
        }

        private object[] GetDataRow(RowIdentifier riNewFk)
        {
            if (riNewFk == null)
                throw new ArgumentNullException(nameof(riNewFk));
            if (!riNewFk.Columns.Any())
                throw new Exception("Failed to return a foreign key value.");

            //La FK (ou unique constraint) n'est pas necessairement la PK donc on réobtient la ligne car
            //BuildExecutionPlan retourne toujours la PK.
            var sqlVar = riNewFk.Columns.Values.First() as SqlVariable;
            if (sqlVar == null)
                throw new Exception();

            var varId = sqlVar.Id;

            foreach (var plan in _executionPlanByServer)
            {
                var dr = plan.Value.InsertSteps.FirstOrDefault(r => r.Variables.Any(v => v.Id == varId));
                if (dr != null)
                    return dr.Datarow;
            }
            throw new Exception();
        }

        private InsertStep CreateExecutionStep(TableIdentifier tiSource, TableIdentifier tiDestination, TableMetadata table, object[] destinationRow, int level)
        {
            var step = new InsertStep
            {
                StepId = _nextStepId++,
                SourceTable = tiSource,
                DestinationTable = tiDestination,
                TableMetadata = table,
                Depth = level
            };

            //Renseignement des variables à générer
            foreach (var col in table.ColumnsDefinition)
            {
                var colName = col.Name;

                var valueToGenerate = ((col.IsPrimary && !col.IsAutoIncrement) || col.IsUniqueKey) && !col.IsForeignKey;
                var pkToGenerate = col.IsPrimary && col.IsAutoIncrement;

                if (valueToGenerate | pkToGenerate)
                {
                    var sqlVar = new SqlVariable(_nextVariableId++);
                    step.Variables.Add(sqlVar);

                    var pos = table.ColumnsDefinition.IndexOf(c => c.Name == colName);
                    destinationRow[pos] = sqlVar;
                }
            }

            step.Datarow = destinationRow;
            AddInsertStep(step);
            return step;
        }

        private void LogStatusChanged(RowIdentifier riSource, int level)
        {
            var clientCopy = riSource.Clone();
            if (StatusChanged != null)
            {
                var args = new StatusChangedEventArgs(Status.Cloning, 0, 0, clientCopy, level);
                StatusChanged(this, args);
            }
        }

        private void LogDerivativeStep(int level)
        {
            if (StatusChanged != null)
            {
                var args = new StatusChangedEventArgs(Status.FetchingDerivatives, 0, 0, null, level);
                StatusChanged(this, args);
            }
        }

        private void GetDerivatives(TableMetadata sourceTable, object[] sourceRow, bool getDerivatives, int level,
                                    Stack<RowIdentifier> rowsGenerating)
        {
            LogDerivativeStep(level + 1);
            var derivativeTable = getDerivatives ? sourceTable.DerivativeTables : sourceTable.DerivativeTables.Where(t => t.Access == DerivativeTableAccess.Forced);

            foreach (var dt in derivativeTable)
            {
                var tableDt = Metadatas.GetTable(dt);
                if (dt.Access == DerivativeTableAccess.Forced && dt.Cascade)
                    getDerivatives = true;
                else if (dt.Access == DerivativeTableAccess.Denied)
                    continue;

                var riDt = new RowIdentifier
                {
                    ServerId = dt.ServerId,
                    Database = dt.Database,
                    Schema = dt.Schema,
                    Table = dt.Table,
                    Columns = sourceTable.BuildDerivativePk(tableDt, sourceRow)
                };

                var rows = ConnectionsContext.Select(riDt);

                //Pour chaque ligne dérivée de la table source
                foreach (var row in rows)
                {
                    riDt.Columns = tableDt.BuildPkFromDataRow(row);

                    rowsGenerating.Push(riDt);
                    BuildExecutionPlan(riDt, getDerivatives, false, level + 1, rowsGenerating);
                    rowsGenerating.Pop();
                }
            }
        }

        private void BuildCircularReferencesPlan()
        {
            foreach (var job in _circularKeyJobs)
            {
                var baseTable = Metadatas.GetTable(job.SourceBaseRowStartPoint);
                var fkTable = Metadatas.GetTable(job.SourceFkRowStartPoint);
                var pkDestinationRow = _keyRelationships.GetKey(job.SourceBaseRowStartPoint);
                var keyDestinationFkRow = _keyRelationships.GetKey(job.SourceFkRowStartPoint);

                var serverDstBaseTable = ExecutionContext.Map[new SehemaIdentifier
                {
                    ServerId = job.SourceBaseRowStartPoint.ServerId,
                    Database = job.SourceBaseRowStartPoint.Database,
                    Schema = job.SourceBaseRowStartPoint.Schema
                }];

                var serverDstFkTable = ExecutionContext.Map[new SehemaIdentifier
                {
                    ServerId = job.SourceFkRowStartPoint.ServerId,
                    Database = job.SourceFkRowStartPoint.Database,
                    Schema = job.SourceFkRowStartPoint.Schema
                }];

                if (job.ForeignKey.Columns.Count != keyDestinationFkRow.Length)
                    throw new Exception("The foreign key defenition is not matching with the values.");

                var fk = new ColumnsWithValue();
                for (var i = 0; i < job.ForeignKey.Columns.Count; i++)
                {
                    var colName = job.ForeignKey.Columns[i].NameTo;
                    var value = keyDestinationFkRow[i];

                    fk.Add(colName, value);
                }

                var riBaseDestination = new RowIdentifier
                {
                    ServerId = serverDstBaseTable.ServerId,
                    Database = serverDstBaseTable.Database,
                    Schema = serverDstBaseTable.Schema,
                    Table = job.SourceBaseRowStartPoint.Table,
                    Columns = baseTable.BuildPkFromRawKey(pkDestinationRow)
                };

                var riFkDestination = new RowIdentifier
                {
                    ServerId = serverDstFkTable.ServerId,
                    Database = serverDstFkTable.Database,
                    Schema = serverDstFkTable.Schema,
                    Table = job.SourceFkRowStartPoint.Table,
                    Columns = fk
                };

                var fkDestinationDataRow = GetDataRow(riFkDestination);
                var modifiedFk = fkTable.BuildKeyFromFkDataRow(job.ForeignKey, fkDestinationDataRow);

                var step = new UpdateStep()
                {
                    StepId = _nextStepId++,
                    DestinationRow = riBaseDestination,
                    ForeignKey = modifiedFk,
                    DestinationTable = riFkDestination
                };

                AddUpdateStep(step);
            }
            _circularKeyJobs.Clear();
        }
        #endregion
    }
}
