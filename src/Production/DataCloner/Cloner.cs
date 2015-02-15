using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using Connection = DataCloner.DataClasse.Cache.Connection;
using DataBuilder = DataCloner.PlugIn.DataBuilder;

namespace DataCloner
{
    internal class CircularKeyJob
    {
        public IRowIdentifier SourceBaseRowStartPoint { get; set; }
        public IRowIdentifier SourceFkRowStartPoint { get; set; }
        public IForeignKey ForeignKey { get; set; }
    }

    public class Cloner
    {
        private const string TempFolderName = "temp";

        private Cache _cache;
        private readonly IQueryDispatcher _dispatcher;
        private readonly Cache.CacheInitialiser _cacheInitialiser;
        private readonly KeyRelationship _keyRelationships;
        private readonly List<CircularKeyJob> _circularKeyJobs;

        public Configuration Config { get; set; }
        public bool SaveToFile { get; set; }
        public string SavePath { get; set; }
        public bool EnforceIntegrity { get; set; }

        public event Action<string> Logger;

        public Cloner()
        {
            _keyRelationships = new KeyRelationship();
            _circularKeyJobs = new List<CircularKeyJob>();
            _dispatcher = new QueryDispatcher();
            _cacheInitialiser = Cache.Init;
        }

        internal Cloner(IQueryDispatcher dispatcher, Cache.CacheInitialiser cacheInit)
        {
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            if (cacheInit == null) throw new ArgumentNullException("cacheInit");

            _keyRelationships = new KeyRelationship();
            _circularKeyJobs = new List<CircularKeyJob>();
            _dispatcher = dispatcher;
            _cacheInitialiser = cacheInit;
        }

        public IRowIdentifier Clone(string application, string mapFrom, string mapTo, int? configId,
                                    IRowIdentifier riSource, bool getDerivatives)
        {
            if (Config == null)
                throw new NullReferenceException("Config");

            _cache = _cacheInitialiser(_dispatcher, Config, application, mapFrom, mapTo, configId);
            _dispatcher[riSource].EnforceIntegrityCheck(EnforceIntegrity);
            var riReturn = SqlTraveler(riSource, getDerivatives, false, 0, new Stack<IRowIdentifier>());

            UpdateCircularReferences();

            return riReturn;
        }

        private IRowIdentifier SqlTraveler(IRowIdentifier riSource, bool getDerivatives, bool shouldReturnFk, int level, Stack<IRowIdentifier> rowsGenerating)
        {
            //var srcRows = riSource.Select();
            var srcRows = _dispatcher.Select(riSource);
            var nbRows = srcRows.Length;
            var table = _cache.GetTable(riSource);
            var autoIncrementPk = table.ColumnsDefinition.Any(c => c.IsAutoIncrement && c.IsPrimary);
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

            LogRow(riSource, level);

            if (shouldReturnFk && nbRows > 1)
                throw new Exception("The foreignkey is not unique!");

            //For each row
            for (var i = 0; i < nbRows; i++)
            {
                var currentRow = srcRows[i];
                var srcKey = table.BuildRawPkFromDataRow(currentRow);

                //Si ligne déjà enregistrée
                var dstKey = _keyRelationships.GetKey(serverDst.ServerId, serverDst.Database, serverDst.Schema, riSource.Table, srcKey);
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

                    //Si le foreignkey est déjà dans la table de destination, on l'utilise
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
                            //var fkRow = riFK.Select();
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
                                var riNewFk = SqlTraveler(riFk, false, true, level + 1, rowsGenerating);
                                rowsGenerating.Pop();

                                //La FK (ou unique constraint) n'est pas necessairement la PK donc on réobtient la ligne car
                                //SqlTraveler retourne toujours la PK.
                                var newFkRow = _dispatcher.Select(riNewFk);

                                //Affecte la clef
                                table.SetFkFromDatarowInDatarow(fkTable, fk, newFkRow, destinationRow);
                            }
                        }
                    }
                }

                //Générer les colonnes qui ont été marquées dans la configuration dataBuilder 
                DataBuilder.BuildDataFromTable(_dispatcher.GetQueryHelper(tiDestination), tiDestination.Database, table, destinationRow);

                //La ligne de destination est prète à l'enregistrement
                //tiDestination.Insert(destinationRow);
                _dispatcher.Insert(tiDestination, destinationRow);

                //Sauve la PK dans la cache
                dstKey = autoIncrementPk ? new[] { _dispatcher[serverDst.ServerId].GetLastInsertedPk() } : table.BuildRawPkFromDataRow(destinationRow);
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

        private void LogRow(IRowIdentifier riSource, int level)
        {
            if (Logger != null)
            {
                var sb = new StringBuilder(new string(' ', 3 * level));
                sb.Append(riSource.Database).Append(".").Append(riSource.Schema).Append(".").Append(riSource.Table).Append(" : (");
                foreach (var col in riSource.Columns)
                    sb.Append(col.Key).Append("=").Append(col.Value).Append(", ");
                sb.Remove(sb.Length - 2, 2);
                sb.Append(")");
                Logger(sb.ToString());
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

                SqlTraveler(riDt, getDerivatives, false, level + 1, new Stack<IRowIdentifier>());
            }
        }

        private void CreateDatabasesFiles()
        {
            var folderPath = Path.Combine(Path.GetDirectoryName(SavePath), TempFolderName);
            var nbFileToCreate = _cache.ServerMap.Select(r => r.Value.ServerId).Distinct().Count();
            int lastIdUsed = _cache.ConnectionStrings.Max(cs => cs.Id);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            for (var i = 0; i < nbFileToCreate; i++)
            {
                var id = lastIdUsed + i + 1;
                var fileName = id + ".sqlite";
                var fullFilePath = Path.Combine(folderPath, fileName);

                //Crer le fichier
                SQLiteConnection.CreateFile(fullFilePath);

                //Crer la string de connection
                _cache.ConnectionStrings.Add(new Connection
                {
                    Id = (short)id,
                    ConnectionString = String.Format("Data Source={0};Version=3;", fullFilePath),
                    ProviderName = "SQLite"
                });

                //_dispatcher.CreateDatabaseFromCache(null, null);
            }
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
    }
}
