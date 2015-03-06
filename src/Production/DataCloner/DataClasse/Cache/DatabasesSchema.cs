using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DataCloner.DataAccess;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;

namespace DataCloner.DataClasse.Cache
{
    /// <summary>
    /// Contient les tables statiques de la base de données
    /// </summary>
    /// <remarks>Optimisé pour la lecture et non pour l'écriture!</remarks>
    internal sealed class DatabasesSchema
    {
        //ServerId / Database / Schema -> TableSchema[]
        private readonly Dictionary<Int16, Dictionary<string, Dictionary<string, TableSchema[]>>> _dic =
            new Dictionary<Int16, Dictionary<string, Dictionary<string, TableSchema[]>>>();

        public TableSchema GetTable(Int16 server, string database, string schema, string table)
        {
            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema))
                return _dic[server][database][schema].FirstOrDefault(t => t.Name == table);
            return null;
        }

        public void Add(Int16 server, string database, string schema, TableSchema table)
        {
            database = database.ToLower();
            schema = schema.ToLower();

            if (!_dic.ContainsKey(server))
                _dic.Add(server, new Dictionary<string, Dictionary<string, TableSchema[]>>());

            if (!_dic[server].ContainsKey(database))
                _dic[server].Add(database, new Dictionary<string, TableSchema[]>());

            if (!_dic[server][database].ContainsKey(schema))
                _dic[server][database].Add(schema, new[] { table });
            else
            {
                if (!_dic[server][database][schema].Contains(table))
                    _dic[server][database][schema] = _dic[server][database][schema].Add(table);
            }
        }

        public bool Remove(Int16 server, string database, string schema, TableSchema table)
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

        public TableSchema[] this[Int16 server, string database, string schema]
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
                    _dic.Add(server, new Dictionary<string, Dictionary<string, TableSchema[]>>());

                if (!_dic[server].ContainsKey(database))
                    _dic[server].Add(database, new Dictionary<string, TableSchema[]>());

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

            if (!reader.Read())
                return;

            //Init first row
            var currentSchema = reader.GetString(0);
            var previousTable = _dic[serverId][database][currentSchema].First(t => t.Name == reader.GetString(1));
            var previousConstraintName = reader.GetString(2);
            var previousConstraint = new ForeignKey
            {
                ServerIdTo = serverId,
                DatabaseTo = database,
                SchemaTo = currentSchema,
                TableTo = reader.GetString(5)
            };

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                var currentTable = reader.GetString(1);
                var currentConstraint = reader.GetString(2);

                //Si on change de constraint
                if (currentTable != previousTable.Name || currentConstraint != previousConstraintName)
                {
                    previousConstraint.Columns = lstForeignKeyColumns.ToArray();
                    lstForeignKeys.Add(previousConstraint);

                    lstForeignKeyColumns = new List<ForeignKeyColumn>();
                    previousConstraint = new ForeignKey
                    {
                        ServerIdTo = serverId,
                        DatabaseTo = database,
                        SchemaTo = currentSchema,
                        TableTo = reader.GetString(5)
                    };
                    previousConstraintName = currentConstraint;
                }

                //Si on change de table
                if (currentTable != previousTable.Name)
                {
                    previousTable.ForeignKeys = lstForeignKeys.ToArray();

                    //Change de table
                    previousTable = _dic[serverId][database][currentSchema].First(t => t.Name == reader.GetString(1));
                    lstForeignKeys = new List<ForeignKey>();
                }

                //Ajoute la colonne
                var colName = reader.GetString(3);
                lstForeignKeyColumns.Add(new ForeignKeyColumn
                {
                    NameFrom = colName,
                    NameTo = reader.GetString(6)
                });

                //Affecte l'indicateur dans le schema
                var col = previousTable.ColumnsDefinition.FirstOrDefault(c => c.Name == colName);
                if (col == null)
                    throw new Exception(string.Format("The column {0} has not been found in the cache for the table {1}.", colName, previousTable.Name));
                col.IsForeignKey = true;
            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstForeignKeyColumns.Count > 0)
            {
                previousConstraint.Columns = lstForeignKeyColumns.ToArray();
                lstForeignKeys.Add(previousConstraint);
                previousTable.ForeignKeys = lstForeignKeys.ToArray();
            }
        }

        public void LoadUniqueKeys(IDataReader reader, Int16 serverId, String database)
        {
            var lstUniqueKeys = new List<UniqueKey>();
            var lstUniqueKeyColumns = new List<string>();

            if (!reader.Read())
                return;

            //Init first row
            var currentSchema = reader.GetString(0);
            var previousTable = _dic[serverId][database][currentSchema].First(t => t.Name == reader.GetString(1));
            var previousConstraintName = reader.GetString(2);
            var previousConstraint = new UniqueKey();

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                var currentTable = reader.GetString(1);
                var currentConstraint = reader.GetString(2);

                //Si on change de constraint
                if (currentTable != previousTable.Name || currentConstraint != previousConstraintName)
                {
                    previousConstraint.Columns = lstUniqueKeyColumns.ToArray();
                    lstUniqueKeys.Add(previousConstraint);

                    lstUniqueKeyColumns = new List<string>();
                    previousConstraint = new UniqueKey();
                    previousConstraintName = currentConstraint;
                }

                //Si on change de table
                if (currentTable != previousTable.Name)
                {
                    previousTable.UniqueKeys = lstUniqueKeys.ToArray();

                    //Change de table
                    previousTable = _dic[serverId][database][currentSchema].First(t => t.Name == reader.GetString(1));
                    lstUniqueKeys = new List<UniqueKey>();
                }

                //Ajoute la colonne
                var colName = reader.GetString(3);
                lstUniqueKeyColumns.Add(colName);

                //Affecte l'indicateur dans le schema
                var col = previousTable.ColumnsDefinition.FirstOrDefault(c => c.Name == colName);
                if (col == null)
                    throw new Exception(string.Format("The column {0} has not been found in the cache for the table {1}.", colName, previousTable.Name));
                col.IsUniqueKey = true;
            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstUniqueKeyColumns.Count > 0)
            {
                previousConstraint.Columns = lstUniqueKeyColumns.ToArray();
                lstUniqueKeys.Add(previousConstraint);
                previousTable.UniqueKeys = lstUniqueKeys.ToArray();
            }
        }

        public void LoadColumns(IDataReader reader, Int16 serverId, String database, SqlTypeToDbTypeConverter dataTypeParser)
        {
            var lstTable = new List<TableSchema>();
            var lstSchemaColumn = new List<ColumnDefinition>();
            var previousTable = new TableSchema();
            string currentSchema;

            if (!reader.Read())
                return;

            //Init first row
            var previousSchema = reader.GetString(0);
            previousTable.Name = reader.GetString(1);

            //Pour chaque ligne
            do
            {
                currentSchema = reader.GetString(0);
                var currentTable = reader.GetString(1);

                //Si on change de table
                if (currentSchema != previousSchema || currentTable != previousTable.Name)
                {
                    previousTable.ColumnsDefinition = lstSchemaColumn.ToArray();
                    lstTable.Add(previousTable);

                    lstSchemaColumn = new List<ColumnDefinition>();
                    previousTable = new TableSchema {Name = currentTable};
                }

                //Si on change de schema
                if (currentSchema != previousSchema)
                {
                    this[serverId, database, currentSchema] = lstTable.ToArray();
                    lstTable = new List<TableSchema>();
                }

                //Ajoute la colonne
                var col = new ColumnDefinition
                {
                    Name = reader.GetString(2),
                    IsPrimary = reader.GetBoolean(4),
                    IsAutoIncrement = reader.GetBoolean(5)
                };
                DbType type;
                string size;
                dataTypeParser(reader.GetString(3), out type, out size);
                col.Type = type;
                col.Size = size;
                lstSchemaColumn.Add(col);

            } while (reader.Read());

            //Ajoute la dernière table / schema
            if (lstSchemaColumn.Count > 0)
            {
                previousTable.ColumnsDefinition = lstSchemaColumn.ToArray();
                lstTable.Add(previousTable);
                this[serverId, database, currentSchema] = lstTable.ToArray();
            }
        }

        /// <summary>
        /// Termine la construction de la cache.
        /// </summary>
        /// <param name="config"></param>
        /// <remarks>Le schéma de la BD doit préalablement avoir été obtenu. GetColumns() et GetForeignKeys()</remarks>
        public void FinalizeCache(ClonerConfiguration config)
        {
            GenerateCommands();
            MergeFkConfig(config);
            GenerateDerivativeTables();
            MergeCacheAndUserConfig(config);
        }

        private void GenerateCommands()
        {
            foreach (var server in _dic)
            {
                foreach (var database in server.Value)
                {
                    foreach (var schema in database.Value)
                    {
                        var nbTables = schema.Value.Length;
                        for (var i = 0; i < nbTables; i++)
                        {
                            var table = schema.Value[i];
                            var sbInsert = new StringBuilder("INSERT INTO ");
							var sbSelect = new StringBuilder("SELECT ");

                            sbInsert.Append(database.Key);
                            if (!string.IsNullOrEmpty(schema.Key))
                                sbInsert.Append(".").Append(schema.Key);
                            sbInsert.Append(".")
                                         .Append(table.Name)
                                         .Append(" (");

                            //Nom des colonnes
                            var nbCols = table.ColumnsDefinition.Length;
                            for (var j = 0; j < nbCols; j++)
                            {
                                //Select
                                sbSelect.Append(table.ColumnsDefinition[j].Name);
                                if (j < nbCols - 1) sbSelect.Append(",");

                                //Insert
                                if (!table.ColumnsDefinition[j].IsAutoIncrement)
                                {
                                    sbInsert.Append(table.ColumnsDefinition[j].Name);
                                    if (j < nbCols - 1) sbInsert.Append(",");
                                }
                            }
                            sbInsert.Append(") VALUES(");

                            //Valeur des colonnes Insert
                            for (var j = 0; j < nbCols; j++)
                            {
                                if (!table.ColumnsDefinition[j].IsAutoIncrement)
                                {
                                    sbInsert.Append("@").Append(table.ColumnsDefinition[j].Name);
                                    if (j < nbCols - 1)
                                        sbInsert.Append(",");
                                }
                            }
                            sbInsert.Append(");");

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

        /// <summary>
        /// Génène les tables dérivées depuis les FK.
        /// </summary>
        /// <remarks>La configuration utilisateur des FK doit avoir été fusionnée à la cache avant la création des tables dérivées.</remarks>
        private void GenerateDerivativeTables()
        {
            foreach (var server in _dic)
            {
                foreach (var database in server.Value)
                {
                    foreach (var schema in database.Value)
                    {
                        foreach (var table in schema.Value)
                        {
                            //On trouve les dérivées de la table
                            foreach (var databaseDeriv in server.Value)
                            {
                                foreach (var schemaDeriv in databaseDeriv.Value)
                                {
                                    foreach (var tableDeriv in schemaDeriv.Value)
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
                                                        ServerId = server.Key,
                                                        Database = databaseDeriv.Key,
                                                        Schema = schemaDeriv.Key,
                                                        Table = tableDeriv.Name
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

        private void MergeFkConfig(ClonerConfiguration config)
        {
            if (config == null)
                return;

            foreach (var server in _dic)
            {
                var serConfig = config.Servers.Find(s => s.Id == server.Key);
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

                                            //On supprime les clefs
                                            foreach (var fkConfig in tblConfig.ForeignKeys.ForeignKeyRemove)
                                            {
                                                foreach (var colConfig in fkConfig.Columns)
                                                {
                                                    for (var j = 0; j < table.ForeignKeys.Count(); j++)
                                                    {
                                                        var fk = table.ForeignKeys[j];

                                                        for (var i = 0; i < fk.Columns.Count(); i++)
                                                        {
                                                            if (fk.Columns[i].NameFrom == colConfig.Name)
                                                            {
                                                                fk.Columns.RemoveAt(i);
                                                                i--;

                                                                if (fk.Columns.Length == 0)
                                                                {
                                                                    table.ForeignKeys.RemoveAt(j);
                                                                    j--;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            //On ajoute les clefs
                                            foreach (var fkConfig in tblConfig.ForeignKeys.ForeignKeyAdd)
                                            {
                                                var newFk = new ForeignKey
                                                {
                                                    ServerIdTo = fkConfig.ServerId,
                                                    DatabaseTo = fkConfig.Database,
                                                    SchemaTo = fkConfig.Schema,
                                                    TableTo = fkConfig.Table,
                                                    Columns = (from fk in fkConfig.Columns select new ForeignKeyColumn { NameFrom = fk.NameFrom, NameTo = fk.NameTo }).ToArray()
                                                };

                                                table.ForeignKeys.Add(newFk);
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

        private void MergeCacheAndUserConfig(ClonerConfiguration config)
        {
            if (config == null)
                return;

            foreach (var server in _dic)
            {
                var serConfig = config.Servers.Find(s => s.Id == server.Key);
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

                                            //Derivative tables
                                            var globalAccess = tblConfig.DerativeTables.GlobalAccess;
                                            var globalCascade = tblConfig.DerativeTables.Cascade;

                                            foreach (var derivTbl in table.DerivativeTables)
                                            {
                                                var derivTblConfig = tblConfig.DerativeTables.DerativeSubTables
                                                                              .FirstOrDefault(t => t.ServerId == derivTbl.ServerId &&
                                                                                              t.Database == derivTbl.Database &&
                                                                                              t.Schema == derivTbl.Schema &&
                                                                                              t.Table == derivTbl.Table);
                                                if (derivTblConfig != null)
                                                {
                                                    derivTbl.Access = derivTblConfig.Access;
                                                    derivTbl.Cascade = derivTblConfig.Cascade;
                                                }
                                                else
                                                {
                                                    derivTbl.Access = globalAccess;
                                                    derivTbl.Cascade = globalCascade;
                                                }
                                            }

                                            //Data builder
                                            foreach (var builderCol in tblConfig.DataBuilders)
                                            {
                                                var col = table.ColumnsDefinition.FirstOrDefault(c => c.Name == builderCol.Name);
                                                if (col != null)
                                                    col.BuilderName = builderCol.BuilderName;
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

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        }

        public static DatabasesSchema Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
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
                        var nbRows = schema.Value.Length;
                        stream.Write(schema.Key);
                        stream.Write(nbRows);
                        for (var i = 0; i < nbRows; i++)
                            schema.Value[i].Serialize(stream);
                    }
                }
            }
        }

        public static DatabasesSchema Deserialize(BinaryReader stream)
        {
            var cTables = new DatabasesSchema();

            var nbServers = stream.ReadInt32();
            for (var n = 0; n < nbServers; n++)
            {
                var serverId = stream.ReadInt16();
                cTables._dic.Add(serverId, new Dictionary<string, Dictionary<string, TableSchema[]>>());

                var nbDatabases = stream.ReadInt32();
                for (var j = 0; j < nbDatabases; j++)
                {
                    var database = stream.ReadString();
                    cTables._dic[serverId].Add(database, new Dictionary<string, TableSchema[]>());

                    var nbSchemas = stream.ReadInt32();
                    for (var k = 0; k < nbSchemas; k++)
                    {
                        var lstTables = new List<TableSchema>();
                        var schema = stream.ReadString();

                        var nbTablesFrom = stream.ReadInt32();
                        for (var l = 0; l < nbTablesFrom; l++)
                            lstTables.Add(TableSchema.Deserialize(stream));

                        cTables._dic[serverId][database].Add(schema, lstTables.ToArray());
                    }
                }
            }
            return cTables;
        }
    }
}