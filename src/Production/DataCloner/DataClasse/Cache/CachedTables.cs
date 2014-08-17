using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

using DataCloner.Framework;
using DataCloner.Enum;
using DataCloner.DataClasse.Configuration;

namespace DataCloner.DataClasse.Cache
{
    /// <summary>
    /// Contient les tables statiques de la base de données
    /// </summary>
    /// <remarks>Optimisé pour la lecture et non pour l'écriture!</remarks>
    internal sealed class CachedTables
    {
        private Dictionary<Int16, Dictionary<string, Dictionary<string, TableDef[]>>> _dic =
            new Dictionary<Int16, Dictionary<string, Dictionary<string, TableDef[]>>>();

        //public bool Contains(Int32 server, string database, string schema, string tableFrom, DerivativeTable tableTo)
        //{
        //    database = database.ToLower();
        //    schema = schema.ToLower();
        //    tableFrom = tableFrom.ToLower();
        //    tableTo.Database = tableTo.Database.ToLower();
        //    tableTo.Schema = tableTo.Schema.ToLower();
        //    tableTo.Table = tableTo.Table.ToLower();

        //    if (_dic.ContainsKey(server) &&
        //        _dic[server].ContainsKey(database) &&
        //        _dic[server][database].ContainsKey(schema) &&
        //        _dic[server][database][schema].ContainsKey(tableFrom) &&
        //        _dic[server][database][schema][tableFrom].Contains(tableTo))
        //        return true;
        //    return false;
        //}

        public TableDef GetTable(Int16 server, string database, string schema, string table)
        {
            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema))
                return _dic[server][database][schema].Where(t => t.Name == table).FirstOrDefault();
            return null;
        }

        public void Add(Int16 server, string database, string schema, TableDef table)
        {
            database = database.ToLower();
            schema = schema.ToLower();

            if (!_dic.ContainsKey(server))
                _dic.Add(server, new Dictionary<string, Dictionary<string, TableDef[]>>());

            if (!_dic[server].ContainsKey(database))
                _dic[server].Add(database, new Dictionary<string, TableDef[]>());

            if (!_dic[server][database].ContainsKey(schema))
                _dic[server][database].Add(schema, new TableDef[] { table });
            else
            {
                if (!_dic[server][database][schema].Contains(table))
                    _dic[server][database][schema] = _dic[server][database][schema].Add(table);
            }
        }

        public bool Remove(Int16 server, string database, string schema, TableDef table)
        {
            database = database.ToLower();
            schema = schema.ToLower();

            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema) &&
                _dic[server][database][schema].Contains(table))
            {
                _dic[server][database][schema] = _dic[server][database][schema].Remove(table);

                if (_dic[server][database][schema].Length == 0)
                {
                    _dic[server][database].Remove(schema);
                    if (!_dic[server][database].Any())
                    {
                        _dic[server].Remove(database);
                        if (!_dic[server].Any())
                        {
                            _dic.Remove(server);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public TableDef[] this[Int16 server, string database, string schema]
        {
            get
            {
                database = database.ToLower();
                schema = schema.ToLower();

                if (_dic.ContainsKey(server) &&
                    _dic[server].ContainsKey(database) &&
                    _dic[server][database].ContainsKey(schema))
                {
                    return _dic[server][database][schema];
                }
                return null;
            }
            set
            {
                database = database.ToLower();
                schema = schema.ToLower();

                if (!_dic.ContainsKey(server))
                    _dic.Add(server, new Dictionary<string, Dictionary<string, TableDef[]>>());

                if (!_dic[server].ContainsKey(database))
                    _dic[server].Add(database, new Dictionary<string, TableDef[]>());

                if (!_dic[server][database].ContainsKey(schema))
                    _dic[server][database].Add(schema, value);
                else
                    _dic[server][database][schema] = value;
            }
        }

        public void LoadForeignKeys(IDataReader reader, Int16 serverId, String database)
        {
            var lstForeignKeys = new List<ForeignKey>();
            var lstForeignKeyColumns = new List<ForeignKeyColumn>();
            var previousTable = new TableDef();
            var fkPreviousConstraint = new ForeignKey();
            string previousConstraint = string.Empty;
            string currentSchema = string.Empty;
            string currentTable = string.Empty;
            string currentConstraint = string.Empty;

            //Init first row
            if (reader.Read())
            {
                currentSchema = reader.GetString(0);
                previousTable = _dic[serverId][database][currentSchema].Where(t => t.Name == reader.GetString(1)).First();
                previousConstraint = reader.GetString(2);
                fkPreviousConstraint = new ForeignKey()
                {
                    ServerIdTo = serverId,
                    DatabaseTo = database,
                    SchemaTo = currentSchema,
                    TableTo = reader.GetString(5)
                };
            }

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                currentTable = reader.GetString(1);
                currentConstraint = reader.GetString(2);

                //Si on change de constraint
                if (currentTable != previousTable.Name || currentConstraint != previousConstraint)
                {
                    fkPreviousConstraint.Columns = lstForeignKeyColumns.ToArray();
                    lstForeignKeys.Add(fkPreviousConstraint);

                    lstForeignKeyColumns = new List<ForeignKeyColumn>();
                    fkPreviousConstraint = new ForeignKey()
                    {
                        ServerIdTo = serverId,
                        DatabaseTo = database,
                        SchemaTo = currentSchema,
                        TableTo = reader.GetString(5)
                    };
                    previousConstraint = currentConstraint;
                }

                //Si on change de table
                if (currentTable != previousTable.Name)
                {
                    previousTable.ForeignKeys = lstForeignKeys.ToArray();

                    //Change de table
                    previousTable = _dic[serverId][database][currentSchema].Where(t => t.Name == reader.GetString(1)).First();
                    lstForeignKeys = new List<ForeignKey>();
                }

                //Ajoute la colonne
                lstForeignKeyColumns.Add(new ForeignKeyColumn()
                {
                    NameFrom = reader.GetString(3),
                    NameTo = reader.GetString(6)
                });
            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstForeignKeyColumns.Count > 0)
            {
                fkPreviousConstraint.Columns = lstForeignKeyColumns.ToArray();
                lstForeignKeys.Add(fkPreviousConstraint);
                previousTable.ForeignKeys = lstForeignKeys.ToArray();
            }
        }

        public void LoadColumns(IDataReader reader, Int16 serverId, String database)
        {
            var lstTable = new List<TableDef>();
            var lstSchemaColumn = new List<SchemaColumn>();
            var previousTable = new TableDef();
            string previousSchema = string.Empty;
            string currentSchema = string.Empty;
            string currentTable;

            //Init first row
            if (reader.Read())
            {
                previousSchema = reader.GetString(0);
                previousTable.Name = reader.GetString(1);
            }

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                currentTable = reader.GetString(1);

                //Si on change de table
                if (currentSchema != previousSchema || currentTable != previousTable.Name)
                {
                    previousTable.SchemaColumns = lstSchemaColumn.ToArray();
                    lstTable.Add(previousTable);

                    lstSchemaColumn = new List<SchemaColumn>();
                    previousTable = new TableDef();
                    previousTable.Name = currentTable;
                }

                //Si on change de schema
                if (currentSchema != previousSchema)
                {
                    this[serverId, database, currentSchema] = lstTable.ToArray();
                    lstTable = new List<TableDef>();
                }

                //Ajoute la colonne
                lstSchemaColumn.Add(new SchemaColumn()
                {
                    Name = reader.GetString(2),
                    Type = reader.GetString(3),
                    IsPrimary = reader.GetBoolean(4),
                    IsForeignKey = reader.GetBoolean(5),
                    IsAutoIncrement = reader.GetBoolean(6)
                });
            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstSchemaColumn.Count > 0)
            {
                previousTable.SchemaColumns = lstSchemaColumn.ToArray();
                lstTable.Add(previousTable);
                this[serverId, database, currentSchema] = lstTable.ToArray();
            }
        }

        public void GenerateCommands()
        {
            foreach (var server in _dic)
            {
                foreach (var database in server.Value)
                {
                    foreach (var schema in database.Value)
                    {
                        int nbTables = schema.Value.Length;
                        for (int i = 0; i < nbTables; i++)
                        {
                            TableDef table = schema.Value[i];
                            StringBuilder sbInsert = new StringBuilder("INSERT INTO ");
                            StringBuilder sbSelect = new StringBuilder("SELECT ");

                            sbInsert.Append(database.Key);
                            if (!string.IsNullOrEmpty(schema.Key))
                                sbInsert.Append(".").Append(schema.Key);
                            sbInsert.Append(".")
                                         .Append(table.Name)
                                         .Append(" (");

                            //Nom des colonnes
                            int nbCols = table.SchemaColumns.Length;
                            for (int j = 0; j < nbCols; j++)
                            {
                                //Select
                                sbSelect.Append(table.SchemaColumns[j].Name);
                                if (j < nbCols - 1) sbSelect.Append(",");

                                //Insert
                                if (!table.SchemaColumns[j].IsAutoIncrement)
                                {
                                    sbInsert.Append(table.SchemaColumns[j].Name);
                                    if (j < nbCols - 1) sbInsert.Append(",");
                                }
                            }
                            sbInsert.Append(") VALUES(");

                            //Valeur des colonnes Insert
                            for (int j = 0; j < nbCols; j++)
                            {
                                if (!table.SchemaColumns[j].IsAutoIncrement)
                                {
                                    sbInsert.Append("@").Append(table.SchemaColumns[j].Name);
                                    if (j < nbCols - 1)
                                        sbInsert.Append(",");
                                }
                            }
                            sbInsert.Append(")");

                            //Finalisation du select
                            sbSelect.Append(" FROM ")
                                    .Append(database.Key);
                            if (!string.IsNullOrEmpty(schema.Key))
                                sbSelect.Append(".").Append(schema.Key);
                            sbSelect.Append(".")
                                    .Append(table.Name);

                            table.InsertCommand = sbInsert.ToString();
                            table.SelectCommand = sbSelect.ToString();
                        }
                    }
                }
            }
        }

        public void GenerateDerivativeTables(ConfigurationXml config)
        {
            //Precompile chaque table dérivée de chaque table
            foreach (var server in _dic)
            {
                foreach (var database in server.Value)
                {
                    foreach (var schema in database.Value)
                    {
                        foreach (var table in schema.Value)
                        {
                            var globalAccess = DerivativeTableAccess.NotSet;
                            var globalCascade = false;

                            //On remplit avec les éléments de la configuration
                            if (config.TableModifiers.Servers.Exists(s => s.Id == server.Key))
                            {
                                var serConfig = config.TableModifiers.Servers.Find(s => s.Id == server.Key);
                                if (serConfig != null)
                                {
                                    var dbConfig = serConfig.Databases.Find(d => d.Name == database.Key);
                                    if (dbConfig != null)
                                    {
                                        var scheConfig = dbConfig.Schemas.Find(s => s.Name == schema.Key);
                                        if (scheConfig != null)
                                        {
                                            var tblConfig = scheConfig.Tables.Find(t => t.Name == table.Name);
                                            if (tblConfig != null)
                                            {
                                                globalAccess = tblConfig.DerativeTablesConfig.GlobalAccess;
                                                globalCascade = tblConfig.DerativeTablesConfig.Cascade;

                                                foreach (var dtConfig in tblConfig.DerativeTablesConfig.DerativeTables)
                                                {
                                                    var dt = new DerivativeTable
                                                    {
                                                        ServerId = dtConfig.ServerId,
                                                        Schema = dtConfig.Schema,
                                                        Database = dtConfig.Database,
                                                        Table = dtConfig.Table
                                                    };

                                                    if (dtConfig.Access == DerivativeTableAccess.NotSet)
                                                        dt.Access = globalAccess;
                                                    else
                                                        dt.Access = dtConfig.Access;

                                                    //if(dtConfig.Cascade)


                                                    table.DerivativeTables = table.DerivativeTables.Add(dt);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (globalAccess != DerivativeTableAccess.Denied)
                            {
                                //On trouve les dérivées de la table
                                foreach (var dbDeriv in server.Value)
                                {
                                    foreach (var schemaDeriv in database.Value)
                                    {
                                        foreach (var tableDeriv in schema.Value)
                                        {
                                            foreach (var fk in tableDeriv.ForeignKeys)
                                            {
                                                //Si correspondance
                                                if (fk.ServerIdTo == server.Key && fk.DatabaseTo == database.Key &&
                                                    fk.SchemaTo == schema.Key && fk.TableTo == table.Name)
                                                {
                                                    //Si non présente
                                                    if (!table.DerivativeTables.Any(t => t.ServerId == fk.ServerIdTo && t.Schema == fk.SchemaTo &&
                                                        t.Database == fk.DatabaseTo && t.Table == fk.TableTo))
                                                    {
                                                        table.DerivativeTables = table.DerivativeTables.Add(new DerivativeTable
                                                        {
                                                            ServerId = fk.ServerIdTo,
                                                            Schema = fk.SchemaTo,
                                                            Database = fk.DatabaseTo,
                                                            Table = fk.TableTo
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void MergeCacheAndUserConfig(ConfigurationXml config)
        {
            foreach (var server in _dic)
            {
                var serConfig = config.TableModifiers.Servers.Find(s => s.Id == server.Key);
                if (serConfig != null)
                {
                    foreach (var database in server.Value)
                    {
                        var dbConfig = serConfig.Databases.Find(d => d.Name == database.Key);
                        if (dbConfig != null)
                        {
                            foreach (var schema in database.Value)
                            {
                                var scheConfig = dbConfig.Schemas.Find(s => s.Name == schema.Key);
                                if (scheConfig != null)
                                {
                                    foreach (var table in schema.Value)
                                    {                                       
                                        var tblConfig = scheConfig.Tables.Find(t => t.Name == table.Name);
                                        if (tblConfig != null)
                                        {
                                            //On affecte les changements de la configuration
                                            table.IsStatic = tblConfig.IsStatic;
                                            
                                        }
                                    }                                   
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        }

        public static CachedTables Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
            int nbRows;
            stream.Write(_dic.Count);
            foreach (var server in _dic)
            {
                stream.Write(server.Key);
                stream.Write(server.Value.Count);
                foreach (var database in server.Value)
                {
                    stream.Write(database.Key);
                    stream.Write(database.Value.Count);
                    foreach (var schema in database.Value)
                    {
                        nbRows = schema.Value.Length;
                        stream.Write(schema.Key);
                        stream.Write(nbRows);
                        for (int i = 0; i < nbRows; i++)
                            schema.Value[i].Serialize(stream);
                    }
                }
            }
        }

        public static CachedTables Deserialize(BinaryReader stream)
        {
            var cTables = new CachedTables();

            int nbServers = stream.ReadInt32();
            for (int n = 0; n < nbServers; n++)
            {
                Int16 serverId = stream.ReadInt16();
                cTables._dic.Add(serverId, new Dictionary<string, Dictionary<string, TableDef[]>>());

                int nbDatabases = stream.ReadInt32();
                for (int j = 0; j < nbDatabases; j++)
                {
                    string database = stream.ReadString();
                    cTables._dic[serverId].Add(database, new Dictionary<string, TableDef[]>());

                    int nbSchemas = stream.ReadInt32();
                    for (int k = 0; k < nbSchemas; k++)
                    {
                        var lstTables = new List<TableDef>();
                        string schema = stream.ReadString();

                        int nbTablesFrom = stream.ReadInt32();
                        for (int l = 0; l < nbTablesFrom; l++)
                            lstTables.Add(TableDef.Deserialize(stream));

                        cTables._dic[serverId][database].Add(schema, lstTables.ToArray());
                    }
                }
            }
            return cTables;
        }
    }
}
