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
    class DataCloner
    {
        private QueryDispatcher _dispatcher;
        private CachedTables _cacheTable;
        private KeyRelationship _keyRelationships;

        //
        public Dictionary<Tuple<Int16, String>, Tuple<Int16, String>> ServerMap { get; set; }

        public DataCloner()
        {
            ServerMap = new Dictionary<Tuple<short, string>, Tuple<short, string>>();
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
            var serverDestination = ServerMap[new Tuple<short,string>(riSource.ServerId, riSource.Database)];
            var riReturn = new RowIdentifier()
            {
                ServerId = serverDestination.Item1,
                Database = serverDestination.Item2,
                Schema = riSource.Schema,
                Table = riSource.Table
            };
            var tiDestination = new TableIdentifier()
            {
                ServerId = serverDestination.Item1,
                Database = serverDestination.Item2,
                Schema = riSource.Schema,
                Table = riSource.Table
            };

            if (shouldReturnFk && nbRows > 1)
                throw new Exception("The foreignkey is not unique!");

            //For each row
            for (int i = 0; i < nbRows; i++)
            {
                var autoIncrementPK = table.SchemaColumns.Where(c => c.IsAutoIncrement).Any(); 
                var currentRow = sourceRows[i];
                object[] sourceKey = table.BuildRawPKFromDataRow(currentRow);
                object[] destKey;

                //Si ligne déjà enregistrée
                destKey = _keyRelationships.GetKey(riSource.ServerId, riSource.Database, riSource.Schema, riSource.Table, sourceKey);
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
                        object[] fkDest = _keyRelationships.GetKey(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo, fk.Columns);
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
                                    }
                                }
                            }
                        }
                        else
                        {
                            var fkDestinationExists = false;
                            var fkTable = _cacheTable.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
                            var colFK = new Dictionary<string, object>();
                            var riFK = new RowIdentifier()
                            {
                                ServerId = fk.ServerIdTo,
                                Database = fk.DatabaseTo,
                                Schema = fk.SchemaTo,
                                Table = fk.TableTo,
                                Columns = colFK
                            };

                            for (int j = 0; j < fk.Columns.Length; j++)
                            {
                                int posTblSource = table.SchemaColumns.IndexOf(c => c.Name == fk.Columns[j].NameFrom);
                                colFK.Add(table.SchemaColumns[posTblSource].Name, currentRow[posTblSource]);
                            }

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
                                var riNewFK =  SqlTraveler(riFK, false, true);
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

                    //Générer la PK
                    if (!autoIncrementPK)
                    {
                                        
                    }

                    //La ligne de destination est prète à l'enregistrement
                    _dispatcher.Insert(tiDestination, destinationRow);

                    if(autoIncrementPK)
                    {
                        destKey = new object[] { _dispatcher.GetLastInsertedPk(riSource.ServerId) };
                        _keyRelationships.SetKey(riSource.ServerId, riSource.Database, riSource.Schema, riSource.Table, sourceKey, destKey);                       
                    }
                    else
                    {
                    
                    }

                    //On affecte la valeur de retour
                    if (shouldReturnFk)
                    {
                        riReturn.Columns = table.BuildPKFromKey(destKey);
                    }
                   

                }
            }



            return riReturn;
        }


    }
}
