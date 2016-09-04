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

        public static Metadatas BuildMetadata(List<Connection> connections,
                                              IQueryProxy queryProxy, 
                                              Behavior behavior)
        {
            var metadatas = FetchMetadata(queryProxy);
            metadatas.GenerateCommands();
            metadatas.MergeForeignKey(behavior);
            metadatas.GenerateDerivativeTables();
            metadatas.MergeBehaviour(behavior);
            return metadatas;
        }

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

        private static void MergeForeignKey(this Metadatas metadatas, Behavior behavior)
        {
            if (behavior == null)
                return;

            foreach (var server in metadatas)
            {
                var serModifier = behavior.DbSettings.Modifiers.Servers.Find(s => s.Id.Equals(server.Key.ToString(), StringComparison.OrdinalIgnoreCase));
                if (serModifier != null)
                {
                    foreach (var database in server.Value)
                    {
                        var dbModifier = serModifier.Databases.Find(d => d.Var.Equals(database.Key, StringComparison.OrdinalIgnoreCase));
                        if (dbModifier != null)
                        {
                            foreach (var schema in database.Value)
                            {
                                var scheModifier = dbModifier.Schemas.Find(s => s.Var.Equals(schema.Key, StringComparison.OrdinalIgnoreCase));
                                if (scheModifier != null)
                                    MergeFkModifierSchema(schema.Value, scheModifier.Tables);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Génène les tables dérivées depuis les FK.
        /// </summary>
        /// <remarks>La configuration utilisateur des FK doit avoir été fusionnée au container avant la création des tables dérivées.</remarks>
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

        private static void MergeFkModifierSchema(SchemaMetadata schemaMetadata, List<Table> tablesModifier)
        {
            foreach (var table in schemaMetadata)
            {
                var tblModifier = tablesModifier.Find(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase));
                if (tblModifier != null)
                {
                    //On affecte les changements de la configuration

                    //On supprime les clefs
                    foreach (var colConfig in tblModifier.ForeignKeys.ForeignKeyRemove.Columns)
                    {
                        for (var j = 0; j < table.ForeignKeys.Count(); j++)
                        {
                            var fk = table.ForeignKeys[j];

                            for (var i = 0; i < fk.Columns.Count(); i++)
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
                    foreach (var fkModifier in tblModifier.ForeignKeys.ForeignKeyAdd)
                    {
                        var newFk = new ForeignKey
                        {
                            ServerIdTo = Int16.Parse(fkModifier.ServerId),
                            DatabaseTo = fkModifier.Database,
                            SchemaTo = fkModifier.Schema,
                            TableTo = fkModifier.Table,
                            Columns = (from fk in fkModifier.Columns select new ForeignKeyColumn { NameFrom = fk.NameFrom, NameTo = fk.NameTo }).ToList()
                        };

                        table.ForeignKeys.Add(newFk);
                    }
                }
            }
        }

        private static void MergeBehaviour(this Metadatas metadatas, Behavior behaviour)
        {
            if (behaviour == null)
                return;

            foreach (var server in metadatas)
            {
                var serModifier = behaviour.Modifiers.Servers.Find(s => s.Id.Equals(server.Key.ToString(), StringComparison.OrdinalIgnoreCase));
                if (serModifier != null)
                {
                    foreach (var database in server.Value)
                    {
                        var dbModifier = serModifier.Databases.Find(d => d.Var.Equals(database.Key, StringComparison.OrdinalIgnoreCase));
                        if (dbModifier != null)
                        {
                            foreach (var schema in database.Value)
                            {
                                var scheModifier = dbModifier.Schemas.Find(s => s.Var.Equals(schema.Key, StringComparison.OrdinalIgnoreCase));
                                if (scheModifier != null)
                                {
                                    foreach (var table in schema.Value)
                                    {
                                        var tblModifier = scheModifier.Tables.Find(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase));
                                        if (tblModifier != null)
                                        {
                                            //On affecte les changements de la configuration
                                            table.IsStatic = tblModifier.IsStatic;

                                            //Derivative tables
                                            var globalAccess = tblModifier.DerativeTables.GlobalAccess;
                                            var globalCascade = tblModifier.DerativeTables.GlobalCascade;

                                            foreach (var derivTbl in table.DerivativeTables)
                                            {
                                                var derivTblModifier = tblModifier.DerativeTables.DerivativeSubTables
                                                                              .FirstOrDefault(t => t.ServerId.Equals(derivTbl.ServerId.ToString(), StringComparison.OrdinalIgnoreCase) &&
                                                                                              t.Database.Equals(derivTbl.Database, StringComparison.OrdinalIgnoreCase) &&
                                                                                              t.Schema.Equals(derivTbl.Schema, StringComparison.OrdinalIgnoreCase) &&
                                                                                              t.Table.Equals(derivTbl.Table, StringComparison.OrdinalIgnoreCase));
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
                                                var col = table.ColumnsDefinition.FirstOrDefault(c => c.Name.Equals(builderCol.Name, StringComparison.OrdinalIgnoreCase));
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
    }
}
