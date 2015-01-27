using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Data.SQLite;

using Murmur;
using DataCloner.PlugIn;

namespace DataCloner
{
    public struct ServerIdentifier
    {
        public Int16 ServerId { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
    }

    public class Cloner
    {
        private const string TEMP_FOLDER_NAME = "temp";

        private CachedTablesSchema _cacheTable;
        private KeyRelationship _keyRelationships;

        public Dictionary<ServerIdentifier, ServerIdentifier> ServerMap { get; set; }
        public bool SaveToFile { get; set; }
        public string SavePath { get; set; }

        public event Action<string> Logger;

        public Cloner()
        {
            ServerMap = new Dictionary<ServerIdentifier, ServerIdentifier>();
        }

        public void Initialize(string cacheName = Configuration.CacheName)
        {
            QueryDispatcher.Initialize(cacheName);
            _cacheTable = QueryDispatcher.Cache.CachedTablesSchema;
            _keyRelationships = new KeyRelationship();

            if (ServerMap == null)
                throw new Exception("ServerMap is not defined! Links source and destination.");

            if (SaveToFile)
                CreateDatabasesFiles();
        }

        public IRowIdentifier SqlTraveler(IRowIdentifier riSource, bool getDerivatives)
        {
            return SqlTraveler(riSource, getDerivatives, false, 0, new Stack<IRowIdentifier>());
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
                        riReturn.Columns = table.BuildPKFromKey(dstKey);
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
                            SetFKInDatarow(table, fk, fkDst, destinationRow);
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
                                Columns = table.BuildFKFromDataRow(fk, currentRow)
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
                                if(rowsGenerating.Contains(riFK))
                                {
                                    //Erreur ..... vilain DBA
                                    //Affecte la FK à NULL ou on en prend une random
                                    //On ajoute la table courante + FK dans une liste de tâches pour réassigner les FK "correctement" 
                                }
                                else
                                {
                                    //Crer la ligne et ses dépendances
                                    rowsGenerating.Push(riFK);
                                    var riNewFK = SqlTraveler(riFK, false, true, level + 1, rowsGenerating);
                                    rowsGenerating.Pop();
                                    
                                    //La FK (ou unique constraint) n'est pas necessairement la PK donc on réobtient la ligne.
                                    var newFKRow = riNewFK.Select();

                                    //Affecte la clef
                                    for (int j = 0; j < fk.Columns.Length; j++)
                                    {
                                        int posTblSourceFK = table.ColumnsDefinition.IndexOf(c => c.Name == fk.Columns[j].NameFrom);
                                        int posTblDestinationPK = fkTable.ColumnsDefinition.IndexOf(c => c.Name == fk.Columns[j].NameTo);

                                        destinationRow[posTblSourceFK] = newFKRow[0][posTblDestinationPK];
                                    }
                                }                                
                            }
                        }
                    }

                    //Générer les colonnes qui ont été marquées dans la configuration dataBuilder 
                    DataBuilder.BuildDataFromTable(tiDestination.GetQueryHelper(), tiDestination.Database, table, destinationRow);

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
                        riReturn.Columns = table.BuildPKFromKey(dstKey);
                    }

                    //On clone les lignes des tables dépendantes
                    GetDerivatives(table, currentRow, getDerivatives, level);
                }
            }

            return riReturn;
        }

        /// <summary>
        /// //On trouve la position de chaque colonne pour affecter la valeur de destination.
        /// </summary>
        private static void SetFKInDatarow(TableSchema table, IForeignKey fkDefinition, object[] fkData, object[] destinationRow)
        {
            for (int j = 0; j < fkDefinition.Columns.Length; j++)
            {
                for (int k = 0; k < table.ColumnsDefinition.Length; k++)
                {
                    if (fkDefinition.Columns[j].NameFrom == table.ColumnsDefinition[k].Name)
                        destinationRow[k] = fkData[j];
                }
            }
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

                SqlTraveler(riDT, getDerivatives, false, level + 1);
            }
        }

        private void CreateDatabasesFiles()
        {
            string folderPath = Path.Combine(Path.GetDirectoryName(SavePath), TEMP_FOLDER_NAME);
            int nbFileToCreate = ServerMap.Select(r => r.Value.ServerId).Distinct().Count();
            int lastIdUsed = QueryDispatcher.Cache.ConnectionStrings.Max(cs => cs.Id);

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
                QueryDispatcher.Cache.ConnectionStrings.Add(new Connection
                {
                    Id = (short)id,
                    ConnectionString = String.Format("Data Source={0};Version=3;", fullFilePath),
                    ProviderName = "SQLite",
                    SameConfigAsId = 0
                });

                //_dispatcher.CreateDatabaseFromCache(null, null);
            }
        }
    }
}
