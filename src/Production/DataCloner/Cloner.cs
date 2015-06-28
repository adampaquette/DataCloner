using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;
using DataBuilder = DataCloner.PlugIn.DataBuilder;

namespace DataCloner
{
	public class Cloner
	{
		private readonly IQueryDispatcher _dispatcher;
		private readonly Cache.CacheInitialiser _cacheInitialiser;
		private readonly KeyRelationship _keyRelationships;
		private readonly List<CircularKeyJob> _circularKeyJobs;
		private readonly Dictionary<Int16, ExecutionPlan> _executionPlanByServer;

	    private int _nextVariableId;
		private int _nextStepId;

	    public Cache Cache;
		public bool EnforceIntegrity { get; set; }
        public bool OptimiseExecutionPlan { get; set; }

		public event StatusChangedEventHandler StatusChanged;
		public event QueryCommitingEventHandler QueryCommiting;

		public Cloner()
		{
			_keyRelationships = new KeyRelationship();
			_circularKeyJobs = new List<CircularKeyJob>();
			_executionPlanByServer = new Dictionary<short, ExecutionPlan>();
			_dispatcher = new QueryDispatcher();
			_cacheInitialiser = Cache.Init;
		}

		internal Cloner(IQueryDispatcher dispatcher, Cache.CacheInitialiser cacheInit)
		{
			if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
			if (cacheInit == null) throw new ArgumentNullException(nameof(cacheInit));

			_keyRelationships = new KeyRelationship();
			_circularKeyJobs = new List<CircularKeyJob>();
			_executionPlanByServer = new Dictionary<short, ExecutionPlan>();
			_dispatcher = dispatcher;
			_cacheInitialiser = cacheInit;
		}

		public void Clear()
		{
            _nextStepId = 0;
            _nextVariableId = 0;

			_keyRelationships.Clear();
			_circularKeyJobs.Clear();
			_executionPlanByServer.Clear();
			DataBuilder.ClearBuildersCache();            
		}

		public void Setup(Application app, int mapId, int? configId)
		{
			_cacheInitialiser(_dispatcher, app, mapId, configId, ref Cache);
		}

		public List<IRowIdentifier> Clone(IRowIdentifier riSource, bool getDerivatives)
		{
			if (riSource == null) throw new ArgumentNullException(nameof(riSource));

			var rowsGenerating = new Stack<IRowIdentifier>();
			rowsGenerating.Push(riSource);

			_dispatcher[riSource].EnforceIntegrityCheck(EnforceIntegrity);

			BuildExecutionPlan(riSource, getDerivatives, false, 0, rowsGenerating);
			BuildCircularReferencesPlan();

            if(OptimiseExecutionPlan)
			    OptimizeExecutionPlans(_executionPlanByServer);

			ExecutePlan(_executionPlanByServer);

			return GetClonedRows(riSource);
		}

		private static void OptimizeExecutionPlans(Dictionary<short, ExecutionPlan> plans)
		{
			var data = new List<object>();

			foreach (var plan in plans)
			{
				plan.Value.InsertSteps.ForEach(s => data.AddRange(s.DataRow));
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
            foreach(var sqlVarRefCount in sqlVarsRefCount)
            {
                if (sqlVarRefCount.Value < 2)
                    sqlVarRefCount.Key.QueryValue = false;
            }

            //var memoryEntriesOptimized = sqlVarsRefCount.Count((sv) => sv.Value < 2); 
        }

		private List<IRowIdentifier> GetClonedRows(IRowIdentifier riSource)
		{
			var clonedRows = new List<IRowIdentifier>();
			var srcRows = _dispatcher.Select(riSource);
		    if (srcRows.Length <= 0) return clonedRows;
		    var table = Cache.GetTable(riSource);

            //By default the destination server is the source if no road is found.
            var serverDst = new ServerIdentifier
            {
                ServerId = riSource.ServerId,
                Database = riSource.Database,
                Schema = riSource.Schema
            };

            if (Cache.ServerMap.ContainsKey(serverDst))
                serverDst = Cache.ServerMap[serverDst];

            foreach (var row in srcRows)
		    {
		        var srcKey = table.BuildRawPkFromDataRow(row);
		        var dstKey = _keyRelationships.GetKey(serverDst.ServerId, serverDst.Database,
		                                              serverDst.Schema, riSource.Table, srcKey);
		        if (dstKey == null) continue;
		        var pkTemp = table.BuildPkFromRawKey(dstKey);

		        //Clone for new reference
		        var clonedPk = new ColumnsWithValue();
		        foreach (var col in pkTemp)
		        {
		            var sqlVar = col.Value as SqlVariable;
		            clonedPk.Add(col.Key, sqlVar != null ? sqlVar.Value : col.Value);
		        }

		        var riReturn = new RowIdentifier
		        {
		            ServerId = serverDst.ServerId,
		            Database = serverDst.Database,
		            Schema = serverDst.Schema,
		            Table = riSource.Table,
		            Columns = clonedPk
		        };

		        //Construit la pk de retour
		        clonedRows.Add(riReturn);
		    }
		    return clonedRows;
		}

		/// <summary>
		/// Build execution plan for the specific source row to be able to clone blazing fast.
		/// </summary>
		/// <param name="riSource">Identify a single or multiples plan to clone.</param>
		/// <param name="getDerivatives">Specify if we clone data related to the input(s) line(s) from other tables.</param>
		/// <param name="shouldReturnFk">Indicate that a source row should only return a single line.</param>
		/// <param name="level">Current recursion level.</param>
		/// <param name="rowsGenerating">Current stack to handle circular foreign keys.</param>
		/// <returns>Always return the primary key of the source row, same if the value queried is a foreign key.</returns>
		private IRowIdentifier BuildExecutionPlan(IRowIdentifier riSource, bool getDerivatives, bool shouldReturnFk, int level,
												  Stack<IRowIdentifier> rowsGenerating)
		{
			var srcRows = _dispatcher.Select(riSource);
			var nbRows = srcRows.Length;
			var table = Cache.GetTable(riSource);

            //By default the destination server is the source if no road is found.
            var serverDst = new ServerIdentifier
            {
                ServerId = riSource.ServerId,
                Database = riSource.Database,
                Schema = riSource.Schema
            };

            if (Cache.ServerMap.ContainsKey(serverDst))
                serverDst = Cache.ServerMap[serverDst];

			var riReturn = new RowIdentifier
			{
				ServerId = serverDst.ServerId,
				Database = serverDst.Database,
				Schema = serverDst.Schema,
				Table = riSource.Table
			};
			var tiDestination = new TableIdentifier
			{
				ServerId = serverDst.ServerId,
				Database = serverDst.Database,
				Schema = serverDst.Schema,
				Table = riSource.Table
			};

			LogStatusChanged(riSource, level);

			if (shouldReturnFk && nbRows > 1)
				throw new Exception("The foreignkey is not unique!");

			//For each row
			for (var i = 0; i < nbRows; i++)
			{
				var currentRow = srcRows[i];
				var srcKey = table.BuildRawPkFromDataRow(currentRow);

				//Si ligne déjà enregistrée
				var dstKey = _keyRelationships.GetKey(serverDst.ServerId, serverDst.Database,
													  serverDst.Schema, riSource.Table, srcKey);
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
						var fkTable = Cache.GetTable(fk);
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
							var fkRow = _dispatcher.Select(riFk);
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
								var nullFk = Enumerable.Repeat<object>(1, fk.Columns.Length).ToArray();
								table.SetFkInDatarow(fk, nullFk, destinationRow);

								//On ajoute une tâche pour réassigner la FK "correctement", une fois que toute la chaîne aura été enregistrée.
								_circularKeyJobs.Add(new CircularKeyJob
								{
									SourceBaseRowStartPoint = new RowIdentifier
									{
										ServerId = riSource.ServerId,
										Database = riSource.Database,
										Schema = riSource.Schema,
										Table = riSource.Table,
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

				var step = CreateExecutionStep(riSource, tiDestination, table, destinationRow, level);

				//Sauve la PK dans la cache
				dstKey = table.BuildRawPkFromDataRow(step.DataRow);
				_keyRelationships.SetKey(riSource.ServerId, riSource.Database, riSource.Schema, riSource.Table, srcKey, dstKey);

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

			//Recopie dans le plan d'exécition pour la performance
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

		private object[] GetDataRow(IRowIdentifier riNewFk)
		{
			if (riNewFk == null)
				throw new ArgumentNullException(nameof(riNewFk));
			if (!riNewFk.Columns.Any())
				throw new Exception("Failed to return a foreign key value.");

			//La FK (ou unique constraint) n'est pas necessairement la PK donc on réobtient la ligne car
			//BuildExecutionPlan retourne toujours la PK.
			var varId = (riNewFk.Columns.Values.First() as SqlVariable)?.Id;

			foreach (var plan in _executionPlanByServer)
			{
				var dr = plan.Value.InsertSteps.FirstOrDefault(r => r.Variables.Any(v => v.Id == varId));
				if (dr != null)
					return dr.DataRow;
			}
			throw new Exception();
		}

		private InsertStep CreateExecutionStep(ITableIdentifier tiSource, ITableIdentifier tiDestination, ITableSchema table, object[] destinationRow, int level)
		{
			var step = new InsertStep
			{
				StepId = _nextStepId++,
				SourceTable = tiSource,
				DestinationTable = tiDestination,
				TableSchema = table,
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

			step.DataRow = destinationRow;
			AddInsertStep(step);
			return step;
		}

		private void LogStatusChanged(IRowIdentifier riSource, int level)
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

		private void GetDerivatives(TableSchema sourceTable, object[] sourceRow, bool getDerivatives, int level, 
                                    Stack<IRowIdentifier> rowsGenerating)
		{
			LogDerivativeStep(level + 1);
			var derivativeTable = getDerivatives ? sourceTable.DerivativeTables : sourceTable.DerivativeTables.Where(t => t.Access == DerivativeTableAccess.Forced);

			foreach (var dt in derivativeTable)
			{
				var tableDt = Cache.GetTable(dt);
				if (dt.Access == DerivativeTableAccess.Forced && dt.Cascade)
					getDerivatives = true;

				var riDt = new RowIdentifier
				{
					ServerId = dt.ServerId,
					Database = dt.Database,
					Schema = dt.Schema,
					Table = dt.Table,
					Columns = sourceTable.BuildDerivativePk(tableDt, sourceRow)
				};

                var rows = _dispatcher.Select(riDt);

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

		private void CreateDatabasesFiles()
		{
			//var folderPath = Path.Combine(Path.GetDirectoryName(SavePath), TempFolderName);
			//var nbFileToCreate = Cache.ServerMap.Select(r => r.Value.ServerId).Distinct().Count();
			//int lastIdUsed = Cache.ConnectionStrings.Max(cs => cs.Id);

			//if (!Directory.Exists(folderPath))
			//    Directory.CreateDirectory(folderPath);

			//for (var i = 0; i < nbFileToCreate; i++)
			//{
			//    var id = lastIdUsed + i + 1;
			//    var fileName = id + ".sqlite";
			//    var fullFilePath = Path.Combine(folderPath, fileName);

			//    //Crer le fichier
			//    SQLiteConnection.CreateFile(fullFilePath);

			//    //Crer la string de connection
			//    Cache.ConnectionStrings.Add(new Connection
			//    {
			//        Id = (short)id,
			//        ConnectionString = String.Format("Data Source={0};Version=3;", fullFilePath),
			//        ProviderName = "SQLite"
			//    });

			//    //_dispatcher.CreateDatabaseFromCache(null, null);
			//}
		}

		private void BuildCircularReferencesPlan()
		{
			foreach (var job in _circularKeyJobs)
			{
				var baseTable = Cache.GetTable(job.SourceBaseRowStartPoint);
				var fkTable = Cache.GetTable(job.SourceFkRowStartPoint);
				var pkDestinationRow = _keyRelationships.GetKey(job.SourceBaseRowStartPoint);
				var keyDestinationFkRow = _keyRelationships.GetKey(job.SourceFkRowStartPoint);

				var serverDstBaseTable = Cache.ServerMap[new ServerIdentifier
				{
					ServerId = job.SourceBaseRowStartPoint.ServerId,
					Database = job.SourceBaseRowStartPoint.Database,
					Schema = job.SourceBaseRowStartPoint.Schema
				}];

				var serverDstFkTable = Cache.ServerMap[new ServerIdentifier
				{
					ServerId = job.SourceFkRowStartPoint.ServerId,
					Database = job.SourceFkRowStartPoint.Database,
					Schema = job.SourceFkRowStartPoint.Schema
				}];

				if (job.ForeignKey.Columns.Length != keyDestinationFkRow.Length)
					throw new Exception("The foreign key defenition is not matching with the values.");

				var fk = new ColumnsWithValue();
				for (var i = 0; i < job.ForeignKey.Columns.Length; i++)
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

		private void ExecutePlan(Dictionary<Int16, ExecutionPlan> planByServer)
		{
			ResetExecutionPlan(planByServer);
			Parallel.ForEach(planByServer, a =>
			{
				var qh = _dispatcher.GetQueryHelper(a.Key);
				qh.QueryCommmiting += QueryCommiting;
				qh.Execute(a.Value);
				qh.QueryCommmiting -= QueryCommiting;
			});
		}

        /// <summary>
        /// We reset the variables for the API to regenerate them.
        /// </summary>
        /// <param name="planByServer"></param>
		private void ResetExecutionPlan(Dictionary<Int16, ExecutionPlan> planByServer)
		{
			foreach (var server in planByServer)
				foreach (var sqlVar in server.Value.Variables)
					sqlVar.Value = null;
		}
	}
}
