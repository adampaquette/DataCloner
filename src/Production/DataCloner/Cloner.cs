using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;
using Connection = DataCloner.DataClasse.Cache.Connection;
using DataBuilder = DataCloner.PlugIn.DataBuilder;
using System.Data.SQLite;

namespace DataCloner
{
    public class Cloner
    {
        private const string TempFolderName = "temp";

        private readonly IQueryDispatcher _dispatcher;
        private readonly Cache.CacheInitialiser _cacheInitialiser;
        private readonly KeyRelationship _keyRelationships;
        private readonly List<CircularKeyJob> _circularKeyJobs;
        private readonly List<ExecutionStep> _executionPlan;

        private Cache _cache;
        private int _nextVariableId;
        private int _nextStepId;

        public bool SaveToFile { get; set; }
        public string SavePath { get; set; }
        public bool EnforceIntegrity { get; set; }

        public event StatusChangedEventHandler StatusChanged;

        public Cloner()
        {
            _keyRelationships = new KeyRelationship();
            _circularKeyJobs = new List<CircularKeyJob>();
            _executionPlan = new List<ExecutionStep>();
            _dispatcher = new QueryDispatcher();
            _cacheInitialiser = Cache.Init;
        }

        internal Cloner(IQueryDispatcher dispatcher, Cache.CacheInitialiser cacheInit)
        {
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            if (cacheInit == null) throw new ArgumentNullException("cacheInit");

            _keyRelationships = new KeyRelationship();
            _circularKeyJobs = new List<CircularKeyJob>();
            _executionPlan = new List<ExecutionStep>();
            _dispatcher = dispatcher;
            _cacheInitialiser = cacheInit;
        }

        public void Clear()
        {
            _keyRelationships.Clear();
            _circularKeyJobs.Clear();
            _executionPlan.Clear();
            DataBuilder.ClearBuildersCache();
        }

        public void Setup(Application app, int mapId, int? configId)
        {
            _cacheInitialiser(_dispatcher, app, mapId, configId, ref _cache);
        }

        public List<IRowIdentifier> Clone(IRowIdentifier riSource, bool getDerivatives)
        {
            _dispatcher[riSource].EnforceIntegrityCheck(EnforceIntegrity);
            BuildExecutionPlan(riSource, getDerivatives, false, 0, new Stack<IRowIdentifier>());

            GenerateSqlInsertScript(_executionPlan);

            //UpdateCircularReferences();

            return GetClonedRows(riSource);
        }

        private List<IRowIdentifier> GetClonedRows(IRowIdentifier riSource)
        {
            var clonedRows = new List<IRowIdentifier>();
            var srcRows = _dispatcher.Select(riSource);
            if (srcRows.Length > 0)
            {
                var table = _cache.GetTable(riSource);
                var serverDst = _cache.ServerMap[new ServerIdentifier
                {
                    ServerId = riSource.ServerId,
                    Database = riSource.Database,
                    Schema = riSource.Schema
                }];

                foreach (var row in srcRows)
                {
                    var srcKey = table.BuildRawPkFromDataRow(row);
                    var dstKey = _keyRelationships.GetKey(serverDst.ServerId, serverDst.Database,
                                                          serverDst.Schema, riSource.Table, srcKey);

                    if (dstKey != null)
                    {
                        var riReturn = new RowIdentifier
                        {
                            ServerId = serverDst.ServerId,
                            Database = serverDst.Database,
                            Schema = serverDst.Schema,
                            Table = riSource.Table,
                            Columns = table.BuildPkFromRawKey(dstKey)
                        };

                        //Construit la pk de retour
                        clonedRows.Add(riReturn);
                    }
                }
            }
            return clonedRows;
        }

        /// <summary>
        /// Build execution plan for the specific source row to be able to clone blazing fast.
        /// </summary>
        /// <param name="riSource">Identify a single or multiples rows to clone.</param>
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
            var table = _cache.GetTable(riSource);
            var serverDst = _cache.ServerMap[new ServerIdentifier
            {
                ServerId = riSource.ServerId,
                Database = riSource.Database,
                Schema = riSource.Schema
            }];
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

                    //Si la foreignkey est déjà dans la table de destination, on l'utilise
                    var fkDst = _keyRelationships.GetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, fkValue);
                    if (fkDst != null)
                        table.SetFkInDatarow(fk, fkDst, destinationRow);
                    else
                    {
                        var fkDestinationExists = false;
                        var fkTable = _cache.GetTable(fk);
                        var riFk = new RowIdentifier
                        {
                            ServerId = fk.ServerIdTo,
                            Database = fk.DatabaseTo,
                            Schema = fk.SchemaTo,
                            Table = fk.TableTo,
                            Columns = table.BuildKeyFromDerivativeDataRow(fk, currentRow)
                        };

                        //On ne copie pas la ligne si la table est statique
                        if (fkTable.IsStatic)
                        {
                            var fkRow = _dispatcher.Select(riFk);
                            fkDestinationExists = fkRow.Length == 1;

                            //Si la ligne existe déjà, on l'utilise
                            if (fkRow.Length > 1)
                                throw new Exception("The FK is not unique.");
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
                                table.SetFkFromDatarowInDatarow(fkTable, fk, newFkRow.DataRow, destinationRow);
                            }
                        }
                    }
                }

                var step = CreateExecutionStep(riSource, tiDestination, table, destinationRow);
                _executionPlan.Add(step);

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
                GetDerivatives(table, currentRow, getDerivatives, level);
            }

            return riReturn;
        }

        private ExecutionStep GetDataRow(IRowIdentifier riNewFk)
        {
            if (riNewFk == null)
                throw new ArgumentNullException(nameof(riNewFk));
            if (!riNewFk.Columns.Any())
                throw new Exception("BuildExecutionPlan failed to return a foreign key value.");

            //La FK (ou unique constraint) n'est pas necessairement la PK donc on réobtient la ligne car
            //BuildExecutionPlan retourne toujours la PK.
            var varId = (riNewFk.Columns.Values.First() as SqlVariable).Id;
            return _executionPlan.FirstOrDefault(r => r.Variables.Any(v => v.Id == varId));
        }

        private ExecutionStep CreateExecutionStep(ITableIdentifier tiSource, ITableIdentifier tiDestination, ITableSchema table, object[] destinationRow)
        {
            var step = new ExecutionStep
            {
                StepId = _nextStepId++,
                SourceTable = tiSource,
                DestinationTable = tiDestination,
                TableSchema = table
            };

            //Renseignement des variables à générer
            foreach (var col in table.ColumnsDefinition)
            {
                var colName = col.Name;

                var valueToGenerate = ((col.IsPrimary && !col.IsAutoIncrement) || col.IsUniqueKey) && !col.IsForeignKey;
                var pkToGenerate = col.IsPrimary && col.IsAutoIncrement;

                if (valueToGenerate | pkToGenerate)
                {
                    var sqlVar = new SqlVariable { Id = _nextVariableId++ };
                    step.Variables.Add(sqlVar);

                    var pos = table.ColumnsDefinition.IndexOf(c => c.Name == colName);
                    destinationRow[pos] = sqlVar;
                }
            }

            step.DataRow = destinationRow;
            return step;
        }

        private void LogStatusChanged(IRowIdentifier riSource, int level)
        {
            if (StatusChanged != null)
            {
                var args = new StatusChangedEventArgs(Status.Cloning, 0, 0, riSource, level);
                StatusChanged(this, args);
            }
        }

        private void GetDerivatives(TableSchema table, object[] sourceRow, bool getDerivatives, int level)
        {
            var derivativeTable = getDerivatives ? table.DerivativeTables : table.DerivativeTables.Where(t => t.Access == DerivativeTableAccess.Forced);

            foreach (var dt in derivativeTable)
            {
                var cachedDt = _cache.GetTable(dt);
                if (dt.Access == DerivativeTableAccess.Forced && dt.Cascade)
                    getDerivatives = true;

                var riDt = new RowIdentifier
                {
                    ServerId = dt.ServerId,
                    Database = dt.Database,
                    Schema = dt.Schema,
                    Table = dt.Table,
                    Columns = table.BuildDerivativePk(cachedDt, sourceRow)
                };

                BuildExecutionPlan(riDt, getDerivatives, false, level + 1, new Stack<IRowIdentifier>());
            }
        }

        private void CreateDatabasesFiles()
        {
            //var folderPath = Path.Combine(Path.GetDirectoryName(SavePath), TempFolderName);
            //var nbFileToCreate = _cache.ServerMap.Select(r => r.Value.ServerId).Distinct().Count();
            //int lastIdUsed = _cache.ConnectionStrings.Max(cs => cs.Id);

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
            //    _cache.ConnectionStrings.Add(new Connection
            //    {
            //        Id = (short)id,
            //        ConnectionString = String.Format("Data Source={0};Version=3;", fullFilePath),
            //        ProviderName = "SQLite"
            //    });

            //    //_dispatcher.CreateDatabaseFromCache(null, null);
            //}
        }

        private void UpdateCircularReferences()
        {
            foreach (var job in _circularKeyJobs)
            {
                var baseTable = _cache.GetTable(job.SourceBaseRowStartPoint);
                var fkTable = _cache.GetTable(job.SourceFkRowStartPoint);
                var pkDestinationRow = _keyRelationships.GetKey(job.SourceBaseRowStartPoint);
                var keyDestinationFkRow = _keyRelationships.GetKey(job.SourceFkRowStartPoint);

                var serverDstBaseTable = _cache.ServerMap[new ServerIdentifier
                {
                    ServerId = job.SourceBaseRowStartPoint.ServerId,
                    Database = job.SourceBaseRowStartPoint.Database,
                    Schema = job.SourceBaseRowStartPoint.Schema
                }];

                var serverDstFkTable = _cache.ServerMap[new ServerIdentifier
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

                var fkDestinationDataRow = _dispatcher.Select(riFkDestination);
                var modifiedFk = fkTable.BuildKeyFromFkDataRow(job.ForeignKey, fkDestinationDataRow[0]);

                _dispatcher[serverDstBaseTable.ServerId].Update(riBaseDestination, modifiedFk);
            }
            _circularKeyJobs.Clear();
        }

        private void GenerateSqlInsertScript(List<ExecutionStep> rows)
        {
            var stepsByConnection = new Dictionary<Int16, List<ExecutionStep>>();

            foreach (var row in rows)
            {
                var connId = row.DestinationTable.ServerId;
                if (!stepsByConnection.ContainsKey(connId))
                    stepsByConnection.Add(connId, new List<ExecutionStep>());

                stepsByConnection[connId].Add(row);
            }

            Parallel.ForEach(stepsByConnection, a => _dispatcher.GetQueryHelper(a.Key).Insert(a.Value));
        }
    }
}
