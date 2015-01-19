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

        private CachedTables _cacheTable;
        private KeyRelationship _keyRelationships;

        public Dictionary<ServerIdentifier, ServerIdentifier> ServerMap { get; set; }
        public bool SaveToFile { get; set; }
        public string SavePath { get; set; }

        public Cloner()
        {
            ServerMap = new Dictionary<ServerIdentifier, ServerIdentifier>();
        }

        public void Initialize(string cacheName = Configuration.CacheName)
        {
            QueryDispatcher.Initialize(cacheName);
            _cacheTable = QueryDispatcher.Cache.CachedTables;
            _keyRelationships = new KeyRelationship();

            if (ServerMap == null)
                throw new Exception("ServerMap is not defined! Links source and destination.");

            if (SaveToFile)
                CreateDatabasesFiles();
        }

        public IRowIdentifier SqlTraveler(IRowIdentifier riSource, bool getDerivatives, bool shouldReturnFk)
        {
            var srcRows = riSource.Select();
            int nbRows = srcRows.Length;
            var table = riSource.GetTable();
            var fks = table.ForeignKeys;
            var autoIncrementPK = table.SchemaColumns.Where(c => c.IsAutoIncrement && c.IsPrimary).Any();
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
                        //Si le foreignkey est déjà dans la table de destination, on l'utilise
                        var fkValue = table.BuildRawFKFromDataRow(fk, currentRow);
                        var fkDst = _keyRelationships.GetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, fkValue);
                        if (fkDst != null)
                        {
                            //On trouve la position de chaque colonne pour affecter la valeur de destination
                            for (int j = 0; j < fk.Columns.Length; j++)
                            {
                                for (int k = 0; k < table.SchemaColumns.Length; k++)
                                {
                                    if (fk.Columns[j].NameFrom == table.SchemaColumns[k].Name)
                                    {
                                        destinationRow[k] = fkDst[j];
                                        break; //TODO : VÉRIFIER SI ÇA BREAK À LA PREMIERE COL. SI OUI = ERREUR
                                    }
                                }
                            }
                        }
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
                                //Crer la ligne et ses dépendances
                                var riNewFK = SqlTraveler(riFK, false, true);

                                //La FK (ou unique constraint) n'est pas necessairement la PK donc on réobtient la ligne.
                                var newFKRow = riNewFK.Select();

                                //Affecte la clef
                                for (int j = 0; j < fk.Columns.Length; j++)
                                {
                                    int posTblSourceFK = table.SchemaColumns.IndexOf(c => c.Name == fk.Columns[j].NameFrom);
                                    int posTblDestinationPK = fkTable.SchemaColumns.IndexOf(c => c.Name == fk.Columns[j].NameTo);

                                    destinationRow[posTblSourceFK] = newFKRow[0][posTblDestinationPK];
                                }
                            }
                        }
                    }

                    //Générer les colonnes qui ont été marquées dans la configuration dataBuilder 
                    DataBuilder.BuildDataFromTable(tiDestination.GetConnection(), table, ref destinationRow);

                    //La ligne de destination est prète à l'enregistrement
                    tiDestination.Insert(destinationRow);

                    if (autoIncrementPK)
                    {
                        dstKey = new object[] { QueryDispatcher.GetQueryHelper(serverDst.ServerId).GetLastInsertedPk() };
                        //table.SetPKFromKey(ref destinationRow, destKey);
                        _keyRelationships.SetKey(riSource.ServerId, riSource.Database, riSource.Schema, riSource.Table, srcKey, dstKey);
                    }
                    else
                    {

                    }

                    //Ajouter les colonnes de contrainte unique dans _keyRelationships
                    //...

                    //On affecte la valeur de retour
                    if (shouldReturnFk)
                    {
                        riReturn.Columns = table.BuildPKFromKey(dstKey);
                    }

                    //On clone les lignes des tables dépendantes
                    GetDerivatives(table, currentRow, getDerivatives);                  
                }
            }

            return riReturn;
        }

        private void GetDerivatives(TableDef table, object[] sourceRow, bool getDerivatives)
        {
            IEnumerable<IDerivativeTable> derivativeTable;

            if (getDerivatives)
                derivativeTable = table.DerivativeTables;
            else
                derivativeTable = table.DerivativeTables.Where(t => t.Access == Enum.DerivativeTableAccess.Forced);

            foreach (var dt in derivativeTable)
            {
                var cachedDT = dt.GetTable();
                if (dt.Access == Enum.DerivativeTableAccess.Forced && dt.Cascade)
                    getDerivatives = true;

                var riDT = new RowIdentifier
                {
                    ServerId = dt.ServerId,
                    Database = dt.Database,
                    Schema = dt.Schema,
                    Table = dt.Table,
                    Columns = table.BuildDerivativePK(cachedDT, sourceRow)
                };

                SqlTraveler(riDT, getDerivatives, false);
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
