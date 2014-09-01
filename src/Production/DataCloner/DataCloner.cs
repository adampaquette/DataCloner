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
        public string Schema { get; set; }
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
            object[][] srcRows = _dispatcher.Select(riSource);
            int nbRows = srcRows.Length;
            var table = _cacheTable.GetTable(Impersonate(riSource.ServerId), riSource.Database, riSource.Schema, riSource.Table);
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
                object[] srcKey = table.BuildRawPKFromDataRow(currentRow);
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
                        object[] fkValue = table.BuildRawFKFromDataRow(fk, currentRow);
                        object[] fkDst = _keyRelationships.GetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, fkValue);
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
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var fkDestinationExists = false;
                            var fkTable = _cacheTable.GetTable(Impersonate(fk.ServerIdTo), fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
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
                        dstKey = new object[] { _dispatcher.GetLastInsertedPk(serverDst.ServerId) };
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
                        var cachedDT = _cacheTable.GetTable(Impersonate(dt.ServerId), dt.Database, dt.Schema, dt.Table);

                        if (dt.Access == Enum.DerivativeTableAccess.Forced && dt.Cascade)
                        {
                            getDerivatives = true;
                        }

                        var riDT = new RowIdentifier
                        {
                            ServerId = dt.ServerId,
                            Database = dt.Database,
                            Schema = dt.Schema,
                            Table = dt.Table,
                            Columns = table.BuildDerivativePK(cachedDT, currentRow)
                        };

                        SqlTraveler(riDT, getDerivatives, false);
                    }
                }
            }

            return riReturn;
        }

        /// <summary>
        /// Impersonnification du schéma
        /// </summary>
        /// <param name="serverId"></param>
        private Int16 Impersonate(Int16 serverId)
        {
            Int16 id =  _dispatcher.Cache.ConnectionStrings.Where(c => c.Id == serverId).First().SameConfigAsId;
            if (id > 0)
                return id;
            return serverId; 
        }
    }
}
