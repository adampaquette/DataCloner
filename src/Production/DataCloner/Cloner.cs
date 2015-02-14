using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;

namespace DataCloner.DataClasse
{
    public struct ServerIdentifier
    {
        public Int16 ServerId { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
    }

    internal class CircularKeyJob
    {
        public IRowIdentifier sourceBaseRowStartPoint { get; set; }
        public IRowIdentifier sourceFKRowStartPoint { get; set; }
        public IForeignKey foreignKey { get; set; }
    }

    public class Cloner
    {
        private const string TEMP_FOLDER_NAME = "temp";

        private Cache.Cache _cache;
        private KeyRelationship _keyRelationships;
        private List<CircularKeyJob> _circularKeyJobs;

        public Dictionary<ServerIdentifier, ServerIdentifier> ServerMap { get; set; }
        public bool SaveToFile { get; set; }
        public string SavePath { get; set; }
        public bool EnforceIntegrity { get; set; }

        public event Action<string> Logger;

        public Cloner()
        {
            ServerMap = new Dictionary<ServerIdentifier, ServerIdentifier>();
            _keyRelationships = new KeyRelationship();
            _circularKeyJobs = new List<CircularKeyJob>();
        }

        public IRowIdentifier SqlTraveler(string application, string mapFrom, string mapTo, int? configId, 
                                          IRowIdentifier riSource, bool getDerivatives)
        {
            Cache.Cache.InitCache(application, mapFrom, mapTo, configId);

            riSource.EnforceIntegrityCheck(EnforceIntegrity);
            var riReturn = SqlTraveler(riSource, getDerivatives, false, 0, new Stack<IRowIdentifier>());

            UpdateCircularReferences();

            return riReturn;
        }

        private IRowIdentifier SqlTraveler(IRowIdentifier riSource, bool getDerivatives, bool shouldReturnFk, int level, Stack<IRowIdentifier> rowsGenerating)
        {
            var srcRows = riSource.Select();
            int nbRows = srcRows.Length;
            var table = riSource.GetTable();
            var fks = table.ForeignKeys;
            var autoIncrementPK = table.ColumnsDefinition.Where(c => c.IsAutoIncrement && c.IsPrimary).Any();
            var serverDst = ServerMap[new ServerIdentifier
            {
                ServerId = riSource.ServerId,
                Database = riSource.Database,
                Schema = riSource.Schema
            }];
            var riReturn = new RowIdentifier()
            {
                ServerId = serverDst.ServerId,
                Database = serverDst.Database,
                Schema = serverDst.Schema,
                Table = riSource.Table
            };
            var tiDestination = new TableIdentifier()
            {
                ServerId = serverDst.ServerId,
                Database = serverDst.Database,
                Schema = serverDst.Schema,
                Table = riSource.Table
            };

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

            if (shouldReturnFk && nbRows > 1)
                throw new Exception("The foreignkey is not unique!");

            //For each row
            for (int i = 0; i < nbRows; i++)
            {
                var currentRow = srcRows[i];
                var srcKey = table.BuildRawPKFromDataRow(currentRow);
                object[] dstKey;

                //Si ligne déjà enregistrée
                dstKey = _keyRelationships.GetKey(serverDst.ServerId, serverDst.Database, serverDst.Schema, riSource.Table, srcKey);
                if (dstKey != null)
                {
                    if (shouldReturnFk)
                    {
                        //Construit la pk de retour
                        riReturn.Columns = table.BuildPKFromRawKey(dstKey);
                        return riReturn;
                    }
                    else
                        continue;
                }
                else
                {
                    var destinationRow = (object[])currentRow.Clone();
                    foreach (var fk in table.ForeignKeys)
                    {
                        //On skip si la FK est null
                        var fkValue = table.BuildRawFKFromDataRow(fk, currentRow);
                        if (fkValue.Contains(DBNull.Value))
                            continue;

                        //Si le foreignkey est déjà dans la table de destination, on l'utilise
                        var fkDst = _keyRelationships.GetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, fkValue);
                        if (fkDst != null)
                            table.SetFKInDatarow(fk, fkDst, destinationRow);
                        else
                        {
                            var fkDestinationExists = false;
                            var fkTable = fk.GetTable();
                            var riFK = new RowIdentifier()
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
                                var fkRow = riFK.Select();
                                fkDestinationExists = fkRow.Length == 1;

                                //Si la ligne existe déjà, on l'utilise
                                if (fkRow.Length > 1)
                                    throw new Exception("The FK is not unique.");
                                else if (fkDestinationExists)
                                {
                                    //Sauve la clef
                                    var colFkObj = fkTable.BuildRawPKFromDataRow(fkRow[0]);
                                    _keyRelationships.SetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, colFkObj, colFkObj);
                                }
                            }

                            //La FK n'existe pas, on la crer
                            if (!fkDestinationExists)
                            {
                                //Si référence circulaire
                                if (rowsGenerating.Contains(riFK))
                                {
                                    //Affecte la FK à 1 pour les contraintes NOT NULL. EnforceIntegrity doit être désactivé.
                                    var nullFK = Enumerable.Repeat<object>(1, fk.Columns.Length).ToArray();
                                    table.SetFKInDatarow(fk, nullFK, destinationRow);

                                    //On ajoute une tâche pour réassigner la FK "correctement", une fois que toute la chaîne aura été enregistrée.
                                    _circularKeyJobs.Add(new CircularKeyJob()
                                    {
                                        sourceBaseRowStartPoint = new RowIdentifier
                                        {
                                            ServerId = riSource.ServerId,
                                            Database = riSource.Database,
                                            Schema = riSource.Schema,
                                            Table = riSource.Table,
                                            Columns = table.BuildPKFromDataRow(currentRow)
                                        },
                                        sourceFKRowStartPoint = riFK,
                                        foreignKey = fk
                                    });
                                }
                                else
                                {
                                    //Crer la ligne et ses dépendances
                                    rowsGenerating.Push(riFK);
                                    var riNewFK = SqlTraveler(riFK, false, true, level + 1, rowsGenerating);
                                    rowsGenerating.Pop();

                                    //La FK (ou unique constraint) n'est pas necessairement la PK donc on réobtient la ligne car
                                    //SqlTraveler retourne toujours une la PK.
                                    var newFKRow = riNewFK.Select();

                                    //Affecte la clef
                                    table.SetFKFromDatarowInDatarow(fkTable, fk, newFKRow, destinationRow);
                                }
                            }
                        }
                    }

                    //Générer les colonnes qui ont été marquées dans la configuration dataBuilder 
                    DataCloner.PlugIn.DataBuilder.BuildDataFromTable(tiDestination.GetQueryHelper(), tiDestination.Database, table, destinationRow);

                    //La ligne de destination est prète à l'enregistrement
                    tiDestination.Insert(destinationRow);

                    //Sauve la PK dans la cache
                    if (autoIncrementPK)
                        dstKey = new object[] { QueryDispatcher.GetQueryHelper(serverDst.ServerId).GetLastInsertedPk() };
                    else
                        dstKey = table.BuildRawPKFromDataRow(destinationRow);
                    _keyRelationships.SetKey(riSource.ServerId, riSource.Database, riSource.Schema, riSource.Table, srcKey, dstKey);

                    //Ajouter les colonnes de contrainte unique dans _keyRelationships
                    //...

                    //On affecte la valeur de retour
                    if (shouldReturnFk)
                    {
                        riReturn.Columns = table.BuildPKFromRawKey(dstKey);
                    }

                    //On clone les lignes des tables dépendantes
                    GetDerivatives(table, currentRow, getDerivatives, level);
                }
            }

            return riReturn;
        }

        private void GetDerivatives(TableSchema table, object[] sourceRow, bool getDerivatives, int level)
        {
            IEnumerable<IDerivativeTable> derivativeTable;

            if (getDerivatives)
                derivativeTable = table.DerivativeTables;
            else
                derivativeTable = table.DerivativeTables.Where(t => t.Access == DerivativeTableAccess.Forced);

            foreach (var dt in derivativeTable)
            {
                var cachedDT = dt.GetTable();
                if (dt.Access == DerivativeTableAccess.Forced && dt.Cascade)
                    getDerivatives = true;

                var riDT = new RowIdentifier
                {
                    ServerId = dt.ServerId,
                    Database = dt.Database,
                    Schema = dt.Schema,
                    Table = dt.Table,
                    Columns = table.BuildDerivativePK(cachedDT, sourceRow)
                };

                SqlTraveler(riDT, getDerivatives, false, level + 1, new Stack<IRowIdentifier>());
            }
        }

        private void CreateDatabasesFiles()
        {
            string folderPath = Path.Combine(Path.GetDirectoryName(SavePath), TEMP_FOLDER_NAME);
            int nbFileToCreate = ServerMap.Select(r => r.Value.ServerId).Distinct().Count();
            int lastIdUsed = _cache.ConnectionStrings.Max(cs => cs.Id);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            for (int i = 0; i < nbFileToCreate; i++)
            {
                int id = lastIdUsed + i + 1;
                string fileName = id.ToString() + ".sqlite";
                string fullFilePath = Path.Combine(folderPath, fileName);

                //Crer le fichier
                SQLiteConnection.CreateFile(fullFilePath);

                //Crer la string de connection
                _cache.ConnectionStrings.Add(new Cache.Connection
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
                TableSchema baseTable = job.sourceBaseRowStartPoint.GetTable();
                TableSchema fkTable = job.sourceFKRowStartPoint.GetTable();
                object[] pkDestinationRow = _keyRelationships.GetKey(job.sourceBaseRowStartPoint);
                object[] keyDestinationFKRow = _keyRelationships.GetKey(job.sourceFKRowStartPoint);

                var serverDstBaseTable = ServerMap[new ServerIdentifier
                {
                    ServerId = job.sourceBaseRowStartPoint.ServerId,
                    Database = job.sourceBaseRowStartPoint.Database,
                    Schema = job.sourceBaseRowStartPoint.Schema
                }];

                var serverDstFKTable = ServerMap[new ServerIdentifier
                {
                    ServerId = job.sourceFKRowStartPoint.ServerId,
                    Database = job.sourceFKRowStartPoint.Database,
                    Schema = job.sourceFKRowStartPoint.Schema
                }];

                if (job.foreignKey.Columns.Length != keyDestinationFKRow.Length)
                    throw new Exception("The foreign key defenition is not matching with the values.");

                var fk = new ColumnsWithValue();
                for (int i = 0; i < job.foreignKey.Columns.Length; i++)
                {
                    var colName = job.foreignKey.Columns[i].NameTo;
                    var value = keyDestinationFKRow[i];

                    fk.Add(colName, value);
                }

                var riBaseDestination = new RowIdentifier
                {
                    ServerId = serverDstBaseTable.ServerId,
                    Database = serverDstBaseTable.Database,
                    Schema = serverDstBaseTable.Schema,
                    Table = job.sourceBaseRowStartPoint.Table,
                    Columns = baseTable.BuildPKFromRawKey(pkDestinationRow)
                };

                var riFKDestination = new RowIdentifier
                {
                    ServerId = serverDstFKTable.ServerId,
                    Database = serverDstFKTable.Database,
                    Schema = serverDstFKTable.Schema,
                    Table = job.sourceFKRowStartPoint.Table,
                    Columns = fk
                };

                var fkDestinationDataRow = riFKDestination.Select();
                var modifiedFk = fkTable.BuildKeyFromFkDataRow(job.foreignKey, fkDestinationDataRow[0]);

                QueryDispatcher.GetQueryHelper(serverDstBaseTable.ServerId).Update(riBaseDestination, modifiedFk);
            }
            _circularKeyJobs.Clear();
        }
    }
}
