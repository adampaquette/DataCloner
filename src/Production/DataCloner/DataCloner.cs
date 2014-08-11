using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Interface;
using DataCloner.Framework;

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

using Murmur;

namespace DataCloner
{
    public struct ServerIdentifier
    {
        public Int16 ServerId { get; set; }
        public string Database { get; set; }
    }

    class DataCloner
    {
        private QueryDispatcher _dispatcher;
        private CachedTables _cacheTable;
        private KeyRelationship _keyRelationships;

        public Dictionary<ServerIdentifier, ServerIdentifier> ServerMap { get; set; }

        public DataCloner()
        {
            ServerMap = new Dictionary<ServerIdentifier, ServerIdentifier>();
        }

        public void Initialize(string cacheName = Configuration.CacheName)
        {
            _dispatcher = new QueryDispatcher();
            _dispatcher.Initialize(cacheName);
            _cacheTable = _dispatcher.Cache.CachedTables;
            _keyRelationships = new KeyRelationship();

            if (ServerMap == null)
                throw new Exception("ServerMap is not defined! Links source and destination.");
        }

        public IRowIdentifier SqlTraveler(IRowIdentifier riSource, bool getDerivatives, bool shouldReturnFk)
        {
            object[][] sourceRows = _dispatcher.Select(riSource);
            int nbRows = sourceRows.Length;
            var table = _cacheTable.GetTable(riSource.ServerId, riSource.Database, riSource.Schema, riSource.Table);
            var fks = table.ForeignKeys;
            var serverDest = ServerMap[new ServerIdentifier { ServerId = riSource.ServerId, Database = riSource.Database }];
            var autoIncrementPK = table.SchemaColumns.Where(c => c.IsAutoIncrement && c.IsPrimary).Any();
            var riReturn = new RowIdentifier()
            {
                ServerId = serverDest.ServerId,
                Database = serverDest.Database,
                Schema = riSource.Schema,
                Table = riSource.Table
            };
            var tiDestination = new TableIdentifier()
            {
                ServerId = serverDest.ServerId,
                Database = serverDest.Database,
                Schema = riSource.Schema,
                Table = riSource.Table
            };

            if (shouldReturnFk && nbRows > 1)
                throw new Exception("The foreignkey is not unique!");

            //For each row
            for (int i = 0; i < nbRows; i++)
            {
                var currentRow = sourceRows[i];
                object[] sourceKey = table.BuildRawPKFromDataRow(currentRow);
                object[] destKey;

                //Si ligne déjà enregistrée
                destKey = _keyRelationships.GetKey(serverDest.ServerId, serverDest.Database, riSource.Schema, riSource.Table, sourceKey);
                if (destKey != null)
                {
                    if (shouldReturnFk)
                    {
                        //Construit la pk de retour
                        riReturn.Columns = table.BuildPKFromKey(destKey);
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
                        object[] fkValue = table.BuildRawFKFromDataRow(fk, currentRow);
                        object[] fkDest = _keyRelationships.GetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, fkValue);
                        if (fkDest != null)
                        {
                            //On trouve la position de chaque colonne pour affecter la valeur de destination
                            for (int j = 0; j < fk.Columns.Length; j++)
                            {
                                for (int k = 0; k < table.SchemaColumns.Length; k++)
                                {
                                    if (fk.Columns[j].NameFrom == table.SchemaColumns[k].Name)
                                    {
                                        destinationRow[k] = fkDest[j];
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var fkDestinationExists = false;
                            var fkTable = _cacheTable.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
                            var riFK = new RowIdentifier()
                            {
                                ServerId = fk.ServerIdTo,
                                Database = fk.DatabaseTo,
                                Schema = fk.SchemaTo,
                                Table = fk.TableTo
                            };
                            riFK.Columns = table.BuildFKFromDataRow(fk, currentRow);

                            //On ne copie pas la ligne si la table est statique
                            if (fkTable.IsStatic)
                            {
                                object[][] fkRow = _dispatcher.Select(riFK);
                                fkDestinationExists = fkRow.Length == 1;

                                //Si la ligne existe déjà, on l'utilise
                                if (fkRow.Length > 1)
                                    throw new Exception("The FK is not unique.");
                                else if (fkDestinationExists)
                                {
                                    //Sauve la clef
                                    var colFkObj = fkTable.BuildRawPKFromDataRow(fkRow[0]);
                                    _keyRelationships.SetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, colFkObj, colFkObj);

                                    //Affecte la clef
                                    for (int j = 0; j < fk.Columns.Length; j++)
                                    {
                                        int posTblSourceFK = table.SchemaColumns.IndexOf(c => c.Name == fk.Columns[j].NameFrom);
                                        int posTblDestinationPK = fkTable.SchemaColumns.IndexOf(c => c.Name == fk.Columns[j].NameTo);

                                        destinationRow[posTblSourceFK] = fkRow[0][posTblDestinationPK];
                                    }
                                }
                            }

                            //La FK n'existe pas, on la crer
                            if (!fkDestinationExists)
                            {
                                //Crer la ligne et ses dépendances
                                var riNewFK = SqlTraveler(riFK, false, true);

                                //La FK (ou unique constraint) n'est pas necessairement la PK donc on réobtient la ligne.
                                object[][] newFKRow = _dispatcher.Select(riNewFK);

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

                    //Récupérer les colonnes qui doivent être générées depuis la configuration dataBuilder 
                    //Pour chaque colonne à générer
                    foreach (var col in table.SchemaColumns)
                    {
                        if ((col.IsPrimary && !col.IsAutoIncrement) || !string.IsNullOrWhiteSpace(col.BuilderName))
                        { 
                            //Générer data
                        }
                    }

                    //La ligne de destination est prète à l'enregistrement
                    _dispatcher.Insert(tiDestination, destinationRow);

                    if (autoIncrementPK)
                    {
                        destKey = new object[] { _dispatcher.GetLastInsertedPk(serverDest.ServerId) };
                        _keyRelationships.SetKey(riSource.ServerId, riSource.Database, riSource.Schema, riSource.Table, sourceKey, destKey);
                    }
                    else
                    {

                    }

                    //Ajouter les colonnes de contrainte unique dans _keyRelationships
                    //...

                    //On affecte la valeur de retour
                    if (shouldReturnFk)
                    {
                        riReturn.Columns = table.BuildPKFromKey(destKey);
                    }

                    /***********************************
                                Get derivative
                     ***********************************/
                    IEnumerable<DerivativeTable> derivativeTable;

                    if (getDerivatives)
                        derivativeTable = table.DerivativeTables;
                    else
                        derivativeTable = table.DerivativeTables.Where(t => t.Access == Enum.DerivativeTableAccess.Forced);

                    foreach (var dt in derivativeTable)
                    {
                        var cachedDT = _cacheTable.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
                        
                        if (dt.Access == Enum.DerivativeTableAccess.Forced && dt.Cascade)
                        {
                            getDerivatives = true;
                        }


                        //var riNewFK = SqlTraveler(riFK, getDerivatives, false);
                    }

                }
            }

            return riReturn;
        }
    }
}
