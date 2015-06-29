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
    /// <remarks>ServerId / Database / Schema -> TableSchema[]</remarks>
    public sealed class DatabasesSchema
        : Dictionary<Int16, Dictionary<string, Dictionary<string, TableSchema[]>>>
    {
        public TableSchema GetTable(Int16 server, string database, string schema, string table)
        {
            if (ContainsKey(server) &&
                this[server].ContainsKey(database) &&
                this[server][database].ContainsKey(schema))
                return this[server][database][schema].FirstOrDefault(t => t.Name == table);
            return null;
        }

        public void Add(Int16 server, string database, string schema, TableSchema table)
        {
            database = database.ToLower();
            schema = schema.ToLower();

            if (!ContainsKey(server))
                Add(server, new Dictionary<string, Dictionary<string, TableSchema[]>>());

            if (!this[server].ContainsKey(database))
                this[server].Add(database, new Dictionary<string, TableSchema[]>());

            if (!this[server][database].ContainsKey(schema))
                this[server][database].Add(schema, new[] { table });
            else
            {
                if (!this[server][database][schema].Contains(table))
                    this[server][database][schema] = this[server][database][schema].Add(table);
            }
        }

        public bool Remove(Int16 server, string database, string schema, TableSchema table)
        {
            database = database.ToLower();
            schema = schema.ToLower();

            if (ContainsKey(server) &&
                this[server].ContainsKey(database) &&
                this[server][database].ContainsKey(schema) &&
                this[server][database][schema].Contains(table))
            {
                this[server][database][schema] = this[server][database][schema].Remove(table);

                if (this[server][database][schema].Length == 0)
                {
                    this[server][database].Remove(schema);
                    if (!this[server][database].Any())
                    {
                        this[server].Remove(database);
                        if (!this[server].Any())
                            Remove(server);
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

                if (ContainsKey(server) &&
                    this[server].ContainsKey(database) &&
                    this[server][database].ContainsKey(schema))
                {
                    return this[server][database][schema];
                }
                return null;
            }
            set
            {
                database = database.ToLower();
                schema = schema.ToLower();

                if (!ContainsKey(server))
                    Add(server, new Dictionary<string, Dictionary<string, TableSchema[]>>());

                if (!this[server].ContainsKey(database))
                    this[server].Add(database, new Dictionary<string, TableSchema[]>());

                if (!this[server][database].ContainsKey(schema))
                    this[server][database].Add(schema, value);
                else
                    this[server][database][schema] = value;
            }
        }

        internal void LoadForeignKeys(IDataReader reader, Int16 serverId, String database)
        {
            var lstForeignKeys = new List<ForeignKey>();
            var lstForeignKeyColumns = new List<ForeignKeyColumn>();

            if (!reader.Read())
                return;

            //Init first row
            var currentSchema = reader.GetString(0);
            var previousTable = this[serverId][database][currentSchema].First(t => t.Name == reader.GetString(1));
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
                    previousTable = this[serverId][database][currentSchema].First(t => t.Name == reader.GetString(1));
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
                    throw new Exception(
                        $"The column {colName} has not been found in the cache for the table {previousTable.Name}.");
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

        internal void LoadUniqueKeys(IDataReader reader, Int16 serverId, String database)
        {
            var lstUniqueKeys = new List<UniqueKey>();
            var lstUniqueKeyColumns = new List<string>();

            if (!reader.Read())
                return;

            //Init first row
            var currentSchema = reader.GetString(0);
            var previousTable = this[serverId][database][currentSchema].First(t => t.Name == reader.GetString(1));
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
                    previousTable = this[serverId][database][currentSchema].First(t => t.Name == reader.GetString(1));
                    lstUniqueKeys = new List<UniqueKey>();
                }

                //Ajoute la colonne
                var colName = reader.GetString(3);
                lstUniqueKeyColumns.Add(colName);

                //Affecte l'indicateur dans le schema
                var col = previousTable.ColumnsDefinition.FirstOrDefault(c => c.Name == colName);
                if (col == null)
                    throw new Exception(
                        $"The column {colName} has not been found in the cache for the table {previousTable.Name}.");
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

        /// <summary>
        /// Load the cache with the columns read from the IDataReader.
        /// The query must have this defenition in order : 
        /// Schema, Table, Column, DataType, Precision, Scale, IsUnsigned, IsPrimaryKey, IsAutoIncrement
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="serverId"></param>
        /// <param name="database"></param>
        /// <param name="typeConverter"></param>
		internal void LoadColumns(IDataReader reader, Int16 serverId, String database, ISqlTypeConverter typeConverter)
        {
            var lstTable = new List<TableSchema>();
            var lstSchemaColumn = new List<ColumnDefinition>();
            string currentSchema;

            if (!reader.Read())
                return;

            //Init first row
            var previousSchema = reader.GetString(0);
            var previousTable = new TableSchema(reader.GetString(1));

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
                    previousTable = new TableSchema(currentTable);
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
                    SqlType = new SqlType
                    {
                        DataType = reader.GetString(3),
                        Precision = reader.GetInt32(4),
                        Scale = reader.GetInt32(5),
                        IsUnsigned = reader.GetBoolean(6)
                    },
                    IsPrimary = reader.GetBoolean(7),
                    IsAutoIncrement = reader.GetBoolean(8)
                };
                col.DbType = typeConverter.ConvertFromSql(col.SqlType);
               
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
        /// Termine la construction de la cache avec les configurations utilisateurs.
        /// </summary>
        /// <param name="behaviour"></param>
        /// <remarks>Le schéma de la BD doit préalablement avoir été obtenu. GetColumns() et GetForeignKeys()</remarks>
        internal void FinalizeCache(ClonerBehaviour behaviour)
        {
            GenerateCommands();
            MergeFk(behaviour);
            GenerateDerivativeTables();
            FinalizeMerge(behaviour);
        }

        /// <summary>
        /// Termine la construction du schema de base de la BD.
        /// </summary>
        /// <remarks>Le schéma de la BD doit préalablement avoir été obtenu. GetColumns() et GetForeignKeys()</remarks>
        internal void FinalizeSchema()
        {
            GenerateDerivativeTables();
        }

        private void GenerateCommands()
        {
            foreach (var server in this)
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
                                sbInsert.Append(".\"").Append(schema.Key).Append('"');
                            sbInsert.Append(".\"")
                                         .Append(table.Name)
                                         .Append("\" (");

                            //Nom des colonnes
                            var nbCols = table.ColumnsDefinition.Length;
                            for (var j = 0; j < nbCols; j++)
                            {
                                //Select
                                sbSelect.Append('"').Append(table.ColumnsDefinition[j].Name).Append('"');
                                if (j < nbCols - 1) sbSelect.Append(",");

                                //Insert
                                if (!table.ColumnsDefinition[j].IsAutoIncrement)
                                {
                                    sbInsert.Append('"').Append(table.ColumnsDefinition[j].Name).Append('"');
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
                            sbSelect.Append(" FROM \"")
                                    .Append(database.Key)
                                    .Append('"');
                            if (!string.IsNullOrEmpty(schema.Key))
                                sbSelect.Append(".\"").Append(schema.Key).Append('"');
                            sbSelect.Append(".\"")
                                    .Append(table.Name)
                                    .Append("\"");

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
            foreach (var server in this)
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

        private void MergeFk(ClonerBehaviour behaviour)
        {
            if (behaviour == null)
                return;

            foreach (var server in this)
            {
                var serModifier = behaviour.Servers.Find(s => s.Id == server.Key.ToString());
                if (serModifier != null)
                {
                    foreach (var database in server.Value)
                    {
                        var dbModifier = serModifier.Databases.Find(d => d.Name == database.Key);
                        if (dbModifier != null)
                        {
                            foreach (var schema in database.Value)
                            {
                                var scheModifier = dbModifier.Schemas.Find(s => s.Name == schema.Key);
                                if (scheModifier != null)
                                    MergeFkModifierSchema(schema.Value, scheModifier.Tables);
                            }
                        }
                    }
                }
            }
        }
        
        private void MergeFkModifierSchema(TableSchema[] tablesCache, List<TableModifier> tablesModifier)
        {
            foreach (var table in tablesCache)
            {
                var tblModifier = tablesModifier.Find(t => t.Name == table.Name);
                if (tblModifier != null)
                {
                    //On affecte les changements de la configuration

                    //On supprime les clefs
                    foreach (var fkModifier in tblModifier.ForeignKeys.ForeignKeyRemove)
                    {
                        foreach (var colConfig in fkModifier.Columns)
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
                    foreach (var fkModifier in tblModifier.ForeignKeys.ForeignKeyAdd)
                    {
                        var newFk = new ForeignKey
                        {
                            ServerIdTo = Int16.Parse(fkModifier.ServerId),
                            DatabaseTo = fkModifier.Database,
                            SchemaTo = fkModifier.Schema,
                            TableTo = fkModifier.Table,
                            Columns = (from fk in fkModifier.Columns select new ForeignKeyColumn { NameFrom = fk.NameFrom, NameTo = fk.NameTo }).ToArray()
                        };

                        table.ForeignKeys.Add(newFk);
                    }
                }
            }            
        }

        private void FinalizeMerge(ClonerBehaviour behaviour)
        {
            if (behaviour == null)
                return;

            foreach (var server in this)
            {
                var serModifier = behaviour.Servers.Find(s => s.Id == server.Key.ToString());
                if (serModifier != null)
                {
                    foreach (var database in server.Value)
                    {
                        var dbModifier = serModifier.Databases.Find(d => d.Name == database.Key);
                        if (dbModifier != null)
                        {
                            foreach (var schema in database.Value)
                            {
                                var scheModifier = dbModifier.Schemas.Find(s => s.Name == schema.Key);
                                if (scheModifier != null)
                                {
                                    foreach (var table in schema.Value)
                                    {
                                        var tblModifier = scheModifier.Tables.Find(t => t.Name == table.Name);
                                        if (tblModifier != null)
                                        {
                                            //On affecte les changements de la configuration
                                            table.IsStatic = tblModifier.IsStatic;

                                            //Derivative tables
                                            var globalAccess = tblModifier.DerativeTables.GlobalAccess;
                                            var globalCascade = tblModifier.DerativeTables.Cascade;

                                            foreach (var derivTbl in table.DerivativeTables)
                                            {
                                                var derivTblModifier = tblModifier.DerativeTables.DerativeSubTables
                                                                              .FirstOrDefault(t => t.ServerId == derivTbl.ServerId.ToString() &&
                                                                                              t.Database == derivTbl.Database &&
                                                                                              t.Schema == derivTbl.Schema &&
                                                                                              t.Table == derivTbl.Table);
                                                if (derivTblModifier != null)
                                                {
                                                    derivTbl.Access = derivTblModifier.Access;
                                                    derivTbl.Cascade = derivTblModifier.Cascade;
                                                }
                                                else
                                                {
                                                    derivTbl.Access = globalAccess;
                                                    derivTbl.Cascade = globalCascade;
                                                }
                                            }

                                            //Data builder
                                            foreach (var builderCol in tblModifier.DataBuilders)
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
            stream.Write(Count);
            foreach (var server in this)
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
                cTables.Add(serverId, new Dictionary<string, Dictionary<string, TableSchema[]>>());

                var nbDatabases = stream.ReadInt32();
                for (var j = 0; j < nbDatabases; j++)
                {
                    var database = stream.ReadString();
                    cTables[serverId].Add(database, new Dictionary<string, TableSchema[]>());

                    var nbSchemas = stream.ReadInt32();
                    for (var k = 0; k < nbSchemas; k++)
                    {
                        var lstTables = new List<TableSchema>();
                        var schema = stream.ReadString();

                        var nbTablesFrom = stream.ReadInt32();
                        for (var l = 0; l < nbTablesFrom; l++)
                            lstTables.Add(TableSchema.Deserialize(stream));

                        cTables[serverId][database].Add(schema, lstTables.ToArray());
                    }
                }
            }
            return cTables;
        }
    }
}