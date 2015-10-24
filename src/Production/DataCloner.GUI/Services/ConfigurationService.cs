using DataCloner.Configuration;
using DataCloner.Framework;
using DataCloner.GUI.Model;
using DataCloner.GUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataCloner.Metadata;

namespace DataCloner.GUI.Services
{
    static class ConfigurationService
    {
        #region Save

        /// <summary>
        /// Update the ViewModels, substract the defaultSchema from the configuration then save the file to disk.
        /// </summary>
        /// <param name="appVM">In-memory application view model.</param>
        /// <param name="defaultSchema">Schema of the plain old database.</param>
        public static void Save(this ApplicationViewModel appVM, Metadata.MetadataPerServer defaultSchema)
        {
            var config = ConfigurationContainer.Load(ConfigurationContainer.ConfigFileName);
            var app = config.Applications.FirstOrDefault(a => a.Id == appVM.Id);

            if (app == null)
            {
                app = new Application();
                app.Id = appVM.Id;
                config.Applications.Add(app);
            }

            app.Name = appVM.Name;
            app.ConnectionStrings = CreateConnectionStrings(appVM.Connections.Connections);
            app.ModifiersTemplates = CreateTemplates(appVM.Templates.ServerModifiers, defaultSchema);

            config.Save(ConfigurationContainer.ConfigFileName);
        }

        private static List<Connection> CreateConnectionStrings(IEnumerable<ConnectionViewModel> connVM)
        {
            var connStrings = new List<Connection>();
            foreach (var conn in connVM)
            {
                connStrings.Add(new Connection
                {
                    Id = conn.Id,
                    Name = conn.Name,
                    ProviderName = conn.ProviderName,
                    ConnectionString = conn.ConnectionString
                });
            }
            return connStrings;
        }

        private static Modifiers CreateTemplates(IEnumerable<ServerModifierModel> serverModifiersVM, MetadataPerServer defaultSchema)
        {
            var userConfigTemplates = new Modifiers();

            foreach (var mergedServer in serverModifiersVM)
            {
                var defaultServerSchema = GetServerSchema(defaultSchema, mergedServer.Id);
                MergeServer(mergedServer, userConfigTemplates.ServerModifiers, defaultServerSchema);
            }

            return userConfigTemplates;
        }

        private static bool MergeServer(Model.ServerModifierModel mergedServer,
                                        List<ServerModifier> userConfigServers,
                                        Dictionary<string, Dictionary<string, TableMetadata[]>> defaultServerSchema)
        {
            var hasChange = false;
            var userConfigServer = userConfigServers.FirstOrDefault(s => s.Id == mergedServer.Id);

            if (defaultServerSchema == null)
                hasChange = true;

            //Add new
            if (userConfigServer == null)
            {
                userConfigServer = new ServerModifier { Id = mergedServer.Id };
                userConfigServers.Add(userConfigServer);
            }

            //Apply changes
            userConfigServer.TemplateId = mergedServer.TemplateId;
            userConfigServer.UseTemplateId = mergedServer.UseTemplateId;

            if (mergedServer.TemplateId != 0 ||
                mergedServer.UseTemplateId != 0)
                hasChange = true;

            foreach (var mergedDatabase in mergedServer.Databases)
            {
                var defaultDatabaseSchema = GetDatabaseSchema(mergedDatabase.Name, defaultServerSchema);
                if (MergeDatabase(mergedDatabase, userConfigServer.Databases, defaultDatabaseSchema))
                    hasChange = true;
            }

            //If no change has been detected with the default config
            if (!hasChange)
                userConfigServers.Remove(userConfigServer);

            return hasChange;
        }

        private static bool MergeDatabase(Model.DatabaseModifierModel mergedDatabase,
                                          List<DatabaseModifier> userConfigDatabases,
                                          Dictionary<string, TableMetadata[]> defaultDatabaseSchema)
        {
            var hasChange = false;
            var userConfigDatabase = userConfigDatabases.FirstOrDefault(d => d.Name == mergedDatabase.Name);

            if (defaultDatabaseSchema == null)
                hasChange = true;

            //Add new
            if (userConfigDatabase == null)
            {
                userConfigDatabase = new DatabaseModifier { Name = mergedDatabase.Name };
                userConfigDatabases.Add(userConfigDatabase);
            }

            //Apply changes
            userConfigDatabase.TemplateId = mergedDatabase.TemplateId;
            userConfigDatabase.UseTemplateId = mergedDatabase.UseTemplateId;

            if (mergedDatabase.TemplateId != 0 ||
                mergedDatabase.UseTemplateId != 0)
                hasChange = true;

            foreach (var mergedSchema in mergedDatabase.Schemas)
            {
                var defaultSchema = GetSchema(mergedSchema.Name, defaultDatabaseSchema);
                if (MergeSchema(mergedSchema, userConfigDatabase.Schemas, defaultSchema))
                    hasChange = true;
            }

            //If no change has been detected with the default config
            if (!hasChange)
                userConfigDatabases.Remove(userConfigDatabase);

            return hasChange;
        }

        private static bool MergeSchema(Model.SchemaModifierModel mergedSchema,
                                        List<SchemaModifier> userConfigSchemas,
                                        TableMetadata[] defaultSchema)
        {
            var hasChange = false;
            var userConfigSchema = userConfigSchemas.FirstOrDefault(s => s.Name == mergedSchema.Name);

            if (defaultSchema == null)
                hasChange = true;

            //Add new
            if (userConfigSchema == null)
            {
                userConfigSchema = new SchemaModifier { Name = mergedSchema.Name };
                userConfigSchemas.Add(userConfigSchema);
            }

            //Apply changes
            userConfigSchema.TemplateId = mergedSchema.TemplateId;
            userConfigSchema.UseTemplateId = mergedSchema.UseTemplateId;

            if (mergedSchema.TemplateId != 0 ||
                mergedSchema.UseTemplateId != 0)
                hasChange = true;

            foreach (var mergedTable in mergedSchema.Tables)
            {
                var defaultTable = defaultSchema.FirstOrDefault(t => t.Name == mergedTable.Name);
                if (MergeTable(mergedTable, userConfigSchema.Tables, defaultTable))
                    hasChange = true;
            }

            //If no change has been detected with the default config
            if (!hasChange)
                userConfigSchemas.Remove(userConfigSchema);

            return hasChange;
        }

        private static bool MergeTable(Model.TableModifierModel mergedTable,
                                       List<TableModifier> userConfigTables,
                                       TableMetadata defaultTable)
        {
            var hasChange = false;
            var userConfigTable = userConfigTables.FirstOrDefault(t => t.Name == mergedTable.Name);

            if (defaultTable == null)
                hasChange = true;

            //Add new
            if (userConfigTable == null)
            {
                userConfigTable = new TableModifier { Name = mergedTable.Name };
                userConfigTables.Add(userConfigTable);
            }

            userConfigTable.IsStatic = mergedTable.IsStatic;
            userConfigTable.DerativeTables.GlobalAccess = mergedTable.DerativeTablesGlobalAccess;
            userConfigTable.DerativeTables.GlobalCascade = mergedTable.DerativeTablesGlobalCascade;

            if (mergedTable.IsStatic ||
                mergedTable.DerativeTablesGlobalAccess != DerivativeTableAccess.NotSet ||
                mergedTable.DerativeTablesGlobalCascade == true)
                hasChange = true;

            //Clear
            userConfigTable.ForeignKeys.ForeignKeyAdd = new List<ForeignKeyAdd>();
            userConfigTable.ForeignKeys.ForeignKeyRemove = new ForeignKeyRemove();
            userConfigTable.DerativeTables.DerativeSubTables = new List<DerivativeSubTable>();
            userConfigTable.DataBuilders = new List<DataBuilder>();

            //Merge FK
            for (int i = mergedTable.ForeignKeys.Count - 1; i >= 0; i--)
            {
                var mergedFk = mergedTable.ForeignKeys[i];
                var defaultFk = defaultTable.ForeignKeys.FirstOrDefault(f => f.ServerIdTo.ToString() == mergedFk.ServerIdTo &&
                                                                             f.DatabaseTo == mergedFk.DatabaseTo &&
                                                                             f.SchemaTo == mergedFk.SchemaTo &&
                                                                             f.TableTo == mergedFk.TableTo);
                if (MergeForeignKey(mergedFk, mergedTable.ForeignKeys, userConfigTable.ForeignKeys, defaultFk, defaultTable.ColumnsDefinition))
                    hasChange = true;
            }

            //Merge derivative table
            for (int i = mergedTable.DerivativeTables.Count - 1; i >= 0; i--)
            {
                var mergedDerivativeTable = mergedTable.DerivativeTables[i];
                var defaultDt = defaultTable.DerivativeTables.FirstOrDefault(d => d.ServerId.ToString() == mergedDerivativeTable.ServerId &&
                                                                                  d.Database == mergedDerivativeTable.Database &&
                                                                                  d.Schema == mergedDerivativeTable.Schema &&
                                                                                  d.Table == mergedDerivativeTable.Table);
                if (MergedDerivativeTable(mergedDerivativeTable, mergedTable.DerivativeTables, userConfigTable.DerativeTables, defaultDt))
                    hasChange = true;
            }

            //Merge data builders
            foreach (var mergedDataBuilder in mergedTable.DataBuilders)
            {
                if (MergeDataBuilders(mergedDataBuilder, userConfigTable.DataBuilders))
                    hasChange = true;
            }

            //Cleaning
            if (!userConfigTable.DataBuilders.Any())
                userConfigTable.DataBuilders = null;
            if (!userConfigTable.ForeignKeys.ForeignKeyAdd.Any())
                userConfigTable.ForeignKeys.ForeignKeyAdd = null;
            if (!userConfigTable.ForeignKeys.ForeignKeyRemove.Columns.Any())
                userConfigTable.ForeignKeys.ForeignKeyRemove = null;
            if (userConfigTable.ForeignKeys.ForeignKeyAdd == null &&
                userConfigTable.ForeignKeys.ForeignKeyRemove == null)
                userConfigTable.ForeignKeys = null;
            if (mergedTable.DerativeTablesGlobalAccess == DerivativeTableAccess.NotSet &&
                mergedTable.DerativeTablesGlobalCascade == false &&
                !userConfigTable.DerativeTables.DerativeSubTables.Any())
                userConfigTable.DerativeTables = null;

            //If no change has been detected with the default config
            if (!hasChange)
                userConfigTables.Remove(userConfigTable);

            return hasChange;
        }

        private static bool MergeForeignKey(Model.ForeignKeyModifierModel mergedFk,
                                            IList<Model.ForeignKeyModifierModel> mergedFks,
                                            ForeignKeys userConfigFk,
                                            IForeignKey defaultFk,
                                            IColumnDefinition[] defaultColumns)
        {
            var hasChange = false;

            if (mergedFk.IsDeleted)
            {
                //Add column in removed section in user config
                if (defaultFk != null)
                {
                    for (int i = mergedFk.Columns.Count - 1; i >= 0; i--)
                    {
                        var mergedFkCol = mergedFk.Columns[i];
                        var isDefaultFkColFound = defaultFk.Columns.Any(f => f.NameFrom == mergedFkCol.NameFrom &&
                                                                             f.NameTo == mergedFkCol.NameTo);
                        //Fk was created by user and is not in the default schema
                        if (!isDefaultFkColFound)
                            mergedFk.Columns.Remove(mergedFkCol);
                        else
                        {
                            var isFkColFound = userConfigFk.ForeignKeyRemove.Columns.Any(f => f.Name == mergedFkCol.NameFrom);
                            if (!isFkColFound)
                            {
                                userConfigFk.ForeignKeyRemove.Columns.Add(new ForeignKeyRemoveColumn { Name = mergedFkCol.NameFrom });
                                hasChange = true;
                            }
                        }
                    }
                }
                //Fk was created by user and is not in the default schema
                else
                    mergedFks.Remove(mergedFk);
            }
            //Add / modify a new foreign key
            else
            {
                var fkAdd = new ForeignKeyAdd
                {
                    ServerId = mergedFk.ServerIdTo,
                    Database = mergedFk.DatabaseTo,
                    Schema = mergedFk.SchemaTo,
                    Table = mergedFk.TableTo
                };

                for (int i = mergedFk.Columns.Count - 1; i >= 0; i--)
                {
                    var mergedFkCol = mergedFk.Columns[i];

                    if (mergedFkCol.IsDeleted)
                    {
                        var isDefaultColFound = defaultColumns.Any(f => f.Name == mergedFkCol.NameFrom);
                        if (isDefaultColFound)
                        {
                            userConfigFk.ForeignKeyRemove.Columns.Add(new ForeignKeyRemoveColumn { Name = mergedFkCol.NameFrom });
                            hasChange = true;
                        }
                        else
                            mergedFk.Columns.Remove(mergedFkCol);
                    }
                    else
                    {
                        var isDefaultFkColFound = defaultFk != null &&
                                                  defaultFk.Columns.Any(f => f.NameFrom == mergedFkCol.NameFrom &&
                                                                             f.NameTo == mergedFkCol.NameTo);
                        if (!isDefaultFkColFound)
                        {
                            fkAdd.Columns.Add(new ForeignKeyColumn
                            {
                                NameFrom = mergedFkCol.NameFrom,
                                NameTo = mergedFkCol.NameTo
                            });
                        }
                    }
                }

                if (fkAdd.Columns.Any())
                {
                    userConfigFk.ForeignKeyAdd.Add(fkAdd);
                    hasChange = true;
                }
            }

            if (!mergedFk.Columns.Any())
                mergedFks.Remove(mergedFk);

            return hasChange;
        }

        private static bool MergedDerivativeTable(DerivativeTableModifierModel mergedDerivativeTable,
                                                 IList<DerivativeTableModifierModel> mergedDerivativeTables,
                                                 DerativeTable derativeTable,
                                                 IDerivativeTable defaultDt)
        {
            var hasChange = false;

            //Was created by user and is not in the default schema
            if (mergedDerivativeTable.IsDeleted && defaultDt == null)
            {
                mergedDerivativeTables.Remove(mergedDerivativeTable);
            }
            else
            {
                if (mergedDerivativeTable.Access != DerivativeTableAccess.NotSet ||
                    mergedDerivativeTable.Cascade == true ||
                    defaultDt == null)
                {
                    derativeTable.DerativeSubTables.Add(new DerivativeSubTable
                    {
                        ServerId = mergedDerivativeTable.ServerId,
                        Database = mergedDerivativeTable.Database,
                        Schema = mergedDerivativeTable.Schema,
                        Table = mergedDerivativeTable.Table,
                        Access = mergedDerivativeTable.Access,
                        Cascade = mergedDerivativeTable.Cascade
                    });

                    hasChange = true;
                }
            }

            return hasChange;
        }


        private static bool MergeDataBuilders(Model.DataBuilderModel mergedDb,
                                              List<DataBuilder> userConfigDataBuilders)
        {
            var hasChange = false;
            var userConfigDataBuilder = userConfigDataBuilders.FirstOrDefault(d => d.Name == mergedDb.ColumnName);

            if (userConfigDataBuilder == null)
            {
                userConfigDataBuilder = new DataBuilder();
                userConfigDataBuilders.Add(userConfigDataBuilder);
            }

            userConfigDataBuilder.Name = mergedDb.ColumnName;
            userConfigDataBuilder.BuilderName = mergedDb.BuilderName;

            if (!string.IsNullOrWhiteSpace(mergedDb.BuilderName))
                hasChange = true;

            //If no change has been detected with the default config
            if (!hasChange)
                userConfigDataBuilders.Remove(userConfigDataBuilder);

            return hasChange;
        }

        #endregion

        #region Load

        public static ApplicationViewModel Load(Application userConfigApp, MetadataPerServer defaultSchema)
        {
            return new ApplicationViewModel
            {
                _id = userConfigApp.Id,
                _name = userConfigApp.Name,
                _connections = LoadConnections(userConfigApp.ConnectionStrings),
                _templates = LoadTemplates(userConfigApp.ModifiersTemplates, defaultSchema),
                _defaultMetadatas = defaultSchema
            };               
        }

        private static ListConnectionViewModel LoadConnections(IEnumerable<Connection> userConfigConns)
        {
            var lstConns = new ListConnectionViewModel();
            var conns = new ObservableCollection<ConnectionViewModel>();
            lstConns._connections = conns;

            foreach (var conn in userConfigConns)
                conns.Add(new ConnectionViewModel(conn));

            return lstConns;
        }

        private static TemplatesViewModel LoadTemplates(Modifiers userConfigTemplates, MetadataPerServer defaultSchema)
        {
            var tVM = new TemplatesViewModel();
            tVM._serverModifiers = new ObservableCollection<ServerModifierModel>();

            //Step 1 : We show every single element from the user's config. Server's templates, database's templates and schema's templates.

            //We add the templates from the user config then merge with the default schema
            foreach (var userConfigServer in userConfigTemplates.ServerModifiers)
            {
                var sVM = new ServerModifierModel
                {
                    _id = userConfigServer.Id,
                    _templateId = userConfigServer.TemplateId,
                    _useTemplateId = userConfigServer.UseTemplateId
                };

                var defaultServerSchema = GetServerSchema(defaultSchema, userConfigServer.Id);




                //tVM._serverModifiers.Add()
            }
            
            //Step 2 : Then if an element from the default schema is not present, we add it to the list. 
            var userConfigServerIds = userConfigTemplates.ServerModifiers.Select(s => s.Id.ExtractVariableValueInt16()).Distinct();
            var defaultServerIds = defaultSchema.Select(s => s.Key).Distinct();
            var serversToAdd = defaultServerIds.Except(userConfigServerIds);

            foreach (var server in serversToAdd)
            {


            }        

            return tVM;
        }



        private static ServerModifierModel LoadServerModifier(KeyValuePair<Int16, Dictionary<string, Dictionary<string, TableMetadata[]>>> defaultSchema)
        {
            return null;
        }

        private static TemplatesViewModel LoadDatabaseTemplates(Modifiers userConfigTemplates, MetadataPerServer defaultSchema)
        {
            
            return null;
        }

        #endregion

            #region Commun

            /// <summary>
            /// Get the default server schema compiled from the database.
            /// </summary>
            /// <param name="defaultSchema">Schema of the plain old database</param>
            /// <param name="serverId">Could be an Id like 0 or a variable like {$KEY{VALUE}} OR {$SERVER_SOURCE{1}}.</param>
        private static Dictionary<string, Dictionary<string, TableMetadata[]>> GetServerSchema(MetadataPerServer defaultSchema, string serverId)
        {
            Int16 id;

            if (serverId == null) return null;

            //If is a serverId
            if (Int16.TryParse(serverId, out id))
            {
                if (defaultSchema.ContainsKey(id))
                    return defaultSchema[id];
            }

            //If is a variable
            id = serverId.ExtractVariableValueInt16();
            if (id != 0 && defaultSchema.ContainsKey(id))
                return defaultSchema[id];

            return null;
        }

        /// <summary>
        /// Get the default database schema compiled from the database.
        /// </summary>
        /// <param name="databaseName">Could be a name like MyDb or a variable like {$KEY{VALUE}} OR {$DATABASE_SOURCE{1}}.</param>
        private static Dictionary<string, TableMetadata[]> GetDatabaseSchema(string databaseName,
            Dictionary<string, Dictionary<string, TableMetadata[]>> serverSchema)
        {
            if (String.IsNullOrEmpty(databaseName)) return null;

            //If is a database name
            if (serverSchema.ContainsKey(databaseName))
                return serverSchema[databaseName];

            //If is a variable
            string name = databaseName.ExtractVariableValue();
            if (name != null && serverSchema.ContainsKey(name))
                return serverSchema[name];

            return null;
        }

        /// <summary>
        /// Get the default schema compiled from the database.
        /// </summary>
        /// <param name="schemaName">Could be a name like DBO or a variable like {$KEY{VALUE}} OR {$SCHEMA_SOURCE{1}}.</param>
        private static TableMetadata[] GetSchema(string schemaName,
            Dictionary<string, TableMetadata[]> databaseSchema)
        {
            if (String.IsNullOrEmpty(schemaName)) return null;

            //If is a schema name
            if (databaseSchema.ContainsKey(schemaName))
                return databaseSchema[schemaName];

            //If is a variable
            string name = schemaName.ExtractVariableValue();
            if (name != null && databaseSchema.ContainsKey(name))
                return databaseSchema[name];

            return null;
        }

        #endregion
    }
}
