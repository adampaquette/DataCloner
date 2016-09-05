using DataCloner.Core.Configuration;
using DataCloner.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCloner.Core.Metadata.Context
{
    public static class MetadataBuilder
    {
        /// <summary>
        /// Build the metadata for a specified behavior.
        /// </summary>
        /// <param name="queryProxy">The proxy used to fetch data</param>
        /// <param name="behavior">A behavior for cloning data</param>
        /// <param name="variables">The compiled cascade variables</param>
        /// <returns>The databases metadatas.</returns>
        public static Metadatas BuildMetadata(IQueryProxy queryProxy, Behavior behavior, HashSet<Variable> variables)
        {
            var metadatas = FetchMetadata(queryProxy);
            metadatas.GenerateCommands();
            metadatas.MergeForeignKey(behavior, variables);
            metadatas.GenerateDerivativeTables();
            metadatas.MergeBehaviour(behavior, variables);
            return metadatas;
        }

        /// <summary>
        /// Get the metadatas from the databases.
        /// </summary>
        /// <param name="queryProxy">The proxy used to fetch data</param>
        /// <returns>The default metadatas from the databases.</returns>
        private static Metadatas FetchMetadata(IQueryProxy queryProxy)
        {
            var metadatas = new Metadatas();

            foreach (var ctx in queryProxy.Contexts)
            {
                foreach (var database in queryProxy.GetDatabasesName(ctx.Key))
                {
                    queryProxy.LoadColumns(ctx.Key, database);
                    queryProxy.LoadForeignKeys(ctx.Key, database);
                    queryProxy.LoadUniqueKeys(ctx.Key, database);
                }
            }
            return metadatas;
        }

        /// <summary>
        /// Generate INSERT and SELECT queries for the tables.
        /// </summary>
        /// <param name="metadatas">Metadatas to generate queries for.</param>
        private static void GenerateCommands(this Metadatas metadatas)
        {
            foreach (var server in metadatas)
            {
                foreach (var database in server.Value)
                {
                    foreach (var schema in database.Value)
                    {
                        foreach (var table in schema.Value)
                        {
                            var sbInsert = new StringBuilder("INSERT INTO ");
                            var sbSelect = new StringBuilder("SELECT ");

                            sbInsert.Append(database.Key);
                            if (!string.IsNullOrEmpty(schema.Key))
                                sbInsert.Append(".\"").Append(schema.Key).Append('"');
                            sbInsert.Append(".\"")
                                         .Append(table.Name)
                                         .Append("\" (");

                            //Nom des colonnes
                            var nbCols = table.ColumnsDefinition.Count;
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
        /// Merge the user configuration with the default metadatas from the servers.
        /// </summary>
        /// <param name="metadatas">Metadatas to generate queries for.</param>
        /// <param name="behavior">A behavior for cloning data</param>
        /// <param name="variables">The compiled cascade variables</param>
        private static void MergeForeignKey(this Metadatas metadatas, Behavior behavior, HashSet<Variable> variables)
        {
            if (behavior == null)
                return;

            foreach (var dbSettings in behavior.DbSettings)
            {
                var dbSettingsVar = variables.First(v => v.Name == dbSettings.Var);

                if (!metadatas.ContainsKey(dbSettingsVar.Server)) continue;
                var server = metadatas[dbSettingsVar.Server];
                if (!server.ContainsKey(dbSettingsVar.Database)) continue;
                var database = server[dbSettingsVar.Database];
                if (!database.ContainsKey(dbSettingsVar.Schema)) continue;
                var schema = database[dbSettingsVar.Schema];
                MergeFkModifierSchema(schema, dbSettings, variables);
            }
        }

        /// <summary>
        /// Merge the user configuration with the default metadatas from the servers.
        /// </summary>
        /// <param name="schemaMetadata">Metadatas to generate queries for.</param>
        /// <param name="dbSettings">User configuration.</param>
        /// <param name="variables">The compiled cascade variables</param>
        private static void MergeFkModifierSchema(SchemaMetadata schemaMetadata, DbSettings dbSettings, HashSet<Variable> variables)
        {
            foreach (var dbSettingsTable in dbSettings.Tables)
            {
                var table = schemaMetadata.FirstOrDefault(t => t.Name.Equals(dbSettingsTable.Name, StringComparison.CurrentCultureIgnoreCase));
                if (table == null) continue;

                //On affecte les changements de la configuration

                //On supprime les clefs
                foreach (var colConfig in dbSettingsTable.ForeignKeys.ForeignKeyRemove.Columns)
                {
                    for (var j = 0; j < table.ForeignKeys.Count; j++)
                    {
                        var fk = table.ForeignKeys[j];

                        for (var i = 0; i < fk.Columns.Count; i++)
                        {
                            if (!fk.Columns[i].NameFrom.Equals(colConfig.Name, StringComparison.OrdinalIgnoreCase))
                                continue;
                            fk.Columns.RemoveAt(i);
                            i--;

                            if (fk.Columns.Count != 0) continue;
                            table.ForeignKeys.RemoveAt(j);
                            j--;
                        }
                    }
                }

                //On ajoute les clefs
                foreach (var fkModifier in dbSettingsTable.ForeignKeys.ForeignKeyAdd)
                {
                    var destinationVar = variables.First(v => v.Name == fkModifier.DestinationVar);

                    var newFk = new ForeignKey
                    {
                        ServerIdTo = destinationVar.Server,
                        DatabaseTo = destinationVar.Database,
                        SchemaTo = destinationVar.Schema,
                        TableTo = fkModifier.TableTo,
                        Columns = (from fk in fkModifier.Columns select new ForeignKeyColumn { NameFrom = fk.NameFrom, NameTo = fk.NameTo }).ToList()
                    };

                    table.ForeignKeys.Add(newFk);
                }
            }
        }

        /// <summary>
        /// Generate derivative tables for the metadatas.
        /// </summary>
        private static void GenerateDerivativeTables(this Metadatas metadatas)
        {
            foreach (var server in metadatas)
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
                                                    table.DerivativeTables.Add(new DerivativeTable
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

        /// <summary>
        /// Merge a user behavior with the default metadatas.
        /// </summary>
        /// <param name="metadatas">Metadatas to generate queries for.</param>
        /// <param name="behavior">A behavior for cloning data</param>
        /// <param name="variables">The compiled cascade variables</param>
        private static void MergeBehaviour(this Metadatas metadatas, Behavior behavior, HashSet<Variable> variables)
        {
            if (behavior == null)
                return;

            foreach (var dbSettings in behavior.DbSettings)
            {
                var dbSettingsVar = variables.First(v => v.Name == dbSettings.Var);

                if (!metadatas.ContainsKey(dbSettingsVar.Server)) continue;
                var server = metadatas[dbSettingsVar.Server];
                if (!server.ContainsKey(dbSettingsVar.Database)) continue;
                var database = server[dbSettingsVar.Database];
                if (!database.ContainsKey(dbSettingsVar.Schema)) continue;
                var schema = database[dbSettingsVar.Schema];

                foreach (var dbSettingsTable in dbSettings.Tables)
                {
                    var table = schema.FirstOrDefault(t => t.Name.Equals(dbSettingsTable.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (table == null) continue;

                    //On affecte les changements de la configuration
                    table.IsStatic = dbSettingsTable.IsStatic == NullableBool.True;

                    //Derivative tables
                    var globalAccess = dbSettingsTable.DerativeTableGlobal.GlobalAccess;
                    var globalCascade = dbSettingsTable.DerativeTableGlobal.GlobalCascade;

                    //Default settings
                    foreach (var derivTbl in table.DerivativeTables)
                    {
                        derivTbl.Access = globalAccess;
                        derivTbl.Cascade = globalCascade == NullableBool.True;
                    }

                    //Override if modified
                    foreach (var dbSettingsDerivativeTable in dbSettingsTable.DerativeTableGlobal.DerivativeTables)
                    {
                        var destinationVar = variables.First(v => v.Name == dbSettingsDerivativeTable.DestinationVar);
                        var derivativeTable = table.DerivativeTables.FirstOrDefault(t => t.ServerId == destinationVar.Server &&
                                                                                    t.Database.Equals(destinationVar.Database,
                                                                                       StringComparison.OrdinalIgnoreCase) &&
                                                                                    t.Schema.Equals(destinationVar.Schema,
                                                                                       StringComparison.OrdinalIgnoreCase) &&
                                                                                    t.Table.Equals(dbSettingsDerivativeTable.Name,
                                                                                       StringComparison.OrdinalIgnoreCase));
                        if (derivativeTable == null) continue;
                        derivativeTable.Access = dbSettingsDerivativeTable.Access;
                        derivativeTable.Cascade = dbSettingsDerivativeTable.Cascade == NullableBool.True;
                    }

                    //Data builder
                    foreach (var builderCol in dbSettingsTable.DataBuilders)
                    {
                        var col = table.ColumnsDefinition.FirstOrDefault(c => c.Name.Equals(builderCol.Name, StringComparison.OrdinalIgnoreCase));
                        if (col != null)
                            col.BuilderName = builderCol.BuilderName;
                    }
                }
            }
        }
    }
}
