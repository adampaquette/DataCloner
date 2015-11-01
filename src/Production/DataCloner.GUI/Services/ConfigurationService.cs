using DataCloner.Configuration;
using DataCloner.GUI.Model;
using DataCloner.GUI.ViewModel;
using DataCloner.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        public static void Save(this ApplicationViewModel appVM, AppMetadata defaultSchema, string path)
        {
            var proj = new ProjectContainer();

            proj.Name = appVM.Name;
            proj.ConnectionStrings = CreateConnectionStrings(appVM.Connections.Connections);
            proj.ModifiersTemplates = CreateTemplates(appVM.Templates.ServerModifiers, defaultSchema);

            proj.Save(path);
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

        private static Modifiers CreateTemplates(IEnumerable<ServerModifierModel> serverModifiersVM, AppMetadata defaultSchema)
        {
            var userConfigTemplates = new Modifiers();

            foreach (var mergedServer in serverModifiersVM)
            {
                var defaultServerSchema = GetServerMetadata(defaultSchema, mergedServer.Id);
                MergeServer(mergedServer, userConfigTemplates.ServerModifiers, defaultServerSchema, defaultSchema);
            }

            return userConfigTemplates;
        }

        private static bool MergeServer(ServerModifierModel mergedServer,
                                        List<ServerModifier> userConfigServers,
                                        ServerMetadata defaultServerSchema,
                                        AppMetadata appMetadata)
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
            userConfigServer.Description = mergedServer.Description;

            if (mergedServer.TemplateId != 0 ||
                mergedServer.UseTemplateId != 0 ||
                !String.IsNullOrEmpty(mergedServer.Description))
                hasChange = true;

            if (mergedServer.Databases != null)
            {
                foreach (var mergedDatabase in mergedServer.Databases)
                {
                    var defaultDatabaseSchema = GetDatabaseMetadata(defaultServerSchema, appMetadata, mergedDatabase.Name);
                    if (MergeDatabase(mergedDatabase, userConfigServer.Databases, defaultDatabaseSchema, appMetadata))
                        hasChange = true;
                }
            }

            //If no change has been detected with the default config
            if (!hasChange)
                userConfigServers.Remove(userConfigServer);

            return hasChange;
        }

        private static bool MergeDatabase(DatabaseModifierModel mergedDatabase,
                                          List<DatabaseModifier> userConfigDatabases,
                                          DatabaseMetadata defaultDatabaseSchema,
                                          AppMetadata appMetadata)
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
            userConfigDatabase.Description = mergedDatabase.Description;

            if (mergedDatabase.TemplateId != 0 ||
                mergedDatabase.UseTemplateId != 0 ||
                !String.IsNullOrEmpty(mergedDatabase.Description))
                hasChange = true;

            if (mergedDatabase.Schemas != null)
            {
                foreach (var mergedSchema in mergedDatabase.Schemas)
                {
                    var defaultSchema = GetSchemaMetadata(defaultDatabaseSchema, appMetadata, mergedSchema.Name);
                    if (MergeSchema(mergedSchema, userConfigDatabase.Schemas, defaultSchema))
                        hasChange = true;
                }
            }

            //If no change has been detected with the default config
            if (!hasChange)
                userConfigDatabases.Remove(userConfigDatabase);

            return hasChange;
        }

        private static bool MergeSchema(SchemaModifierModel mergedSchema,
                                        List<SchemaModifier> userConfigSchemas,
                                        SchemaMetadata defaultSchema)
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
            userConfigSchema.Description = mergedSchema.Description;

            if (mergedSchema.TemplateId != 0 ||
                mergedSchema.UseTemplateId != 0 ||
                !String.IsNullOrEmpty(mergedSchema.Description))
                hasChange = true;

            if (mergedSchema.Tables != null)
            {
                foreach (var mergedTable in mergedSchema.Tables)
                {
                    var defaultTable = defaultSchema.FirstOrDefault(t => t.Name == mergedTable.Name);
                    if (MergeTable(mergedTable, userConfigSchema.Tables, defaultTable))
                        hasChange = true;
                }
            }

            //If no change has been detected with the default config
            if (!hasChange)
                userConfigSchemas.Remove(userConfigSchema);

            return hasChange;
        }

        private static bool MergeTable(TableModifierModel mergedTable,
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

        private static bool MergeForeignKey(ForeignKeyModifierModel mergedFk,
                                            IList<ForeignKeyModifierModel> mergedFks,
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
                            fkAdd.Columns.Add(new DataCloner.Configuration.ForeignKeyColumn
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


        private static bool MergeDataBuilders(DataBuilderModel mergedDb,
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

        public static ApplicationViewModel Load(ProjectContainer proj, AppMetadata defaultAppMetadata)
        {
            return new ApplicationViewModel
            {
                _name = proj.Name,
                _connections = LoadConnections(proj.ConnectionStrings),
                _templates = LoadTemplates(proj.ModifiersTemplates, defaultAppMetadata),
                _defaultMetadatas = defaultAppMetadata
            };
        }

        private static ListConnectionViewModel LoadConnections(IEnumerable<Connection> userConfigConns)
        {
            var lstConns = new ListConnectionViewModel()
            {
                _connections = new ObservableCollection<ConnectionViewModel>()
            };

            foreach (var conn in userConfigConns)
            {
                lstConns._connections.Add(new ConnectionViewModel
                {
                    _id = conn.Id,
                    _name = conn.Name,
                    _connectionString = conn.ConnectionString,
                    _providerName = conn.ProviderName
                });
            }

            return lstConns;
        }

        private static TemplatesViewModel LoadTemplates(Modifiers userConfigTemplates, AppMetadata appMetadata)
        {
            var tVM = new TemplatesViewModel
            {
                _serverModifiers = LoadServerTemplates(userConfigTemplates.ServerModifiers, appMetadata),
                _databaseModifiers = new ObservableCollection<DatabaseModifierModel>(),
                _schemaModifiers = new ObservableCollection<SchemaModifierModel>()
            };

            return tVM;
        }

        private static ObservableCollection<ServerModifierModel> LoadServerTemplates(List<ServerModifier> userConfigTemplates, AppMetadata appMetadata)
        {
            var returnVM = new ObservableCollection<ServerModifierModel>();

            //We show every single element from the user's config. 
            var serversToShow = userConfigTemplates;

            //We add the elements from the default metadata if not present.
            var userConfigDistinctServers = userConfigTemplates.Select(s => s.Id.GetServer()).Distinct();
            var serversToAdd = appMetadata.Select(s => s.Key).Distinct().Except(userConfigDistinctServers);
            serversToShow.AddRange(from s in serversToAdd select new ServerModifier { Id = s.ToString() });

            foreach (var server in serversToShow)
            {
                var defaultSrvMetadata = GetServerMetadata(appMetadata, server.Id);

                returnVM.Add(new ServerModifierModel
                {
                    _id = server.Id,
                    _templateId = server.TemplateId,
                    _useTemplateId = server.UseTemplateId,
                    _description = server.Description,
                    _databases = LoadDatabaseTemplates(server.Databases, defaultSrvMetadata, appMetadata)
                });
            }

            return returnVM;
        }

        private static ObservableCollection<DatabaseModifierModel> LoadDatabaseTemplates(List<DatabaseModifier> userConfigTemplates, 
            ServerMetadata serverMetadata, AppMetadata appMetadata)
        {
            var returnVM = new ObservableCollection<DatabaseModifierModel>();

            //We show every single element from the user's config. 
            var databasesToShow = userConfigTemplates;

            //We add the elements from the default metadata if not present.
            var userConfigDistinctDatabases = userConfigTemplates.Select(s => s.Name.GetDatabase()).Distinct();
            var databasesToAdd = serverMetadata.Select(s => s.Key).Distinct().Except(userConfigDistinctDatabases);
            databasesToShow.AddRange(from d in databasesToAdd select new DatabaseModifier { Name = d });

            foreach (var database in databasesToShow)
            {
                var defaultDbMetadata = GetDatabaseMetadata(serverMetadata, appMetadata, database.Name);

                returnVM.Add(new DatabaseModifierModel
                {
                    _name = database.Name,
                    _templateId = database.TemplateId,
                    _useTemplateId = database.UseTemplateId,
                    _description = database.Description,
                    _schemas = LoadSchemaTemplates(database.Schemas, defaultDbMetadata, appMetadata)
                });
            }

            return returnVM;
        }

        private static ObservableCollection<SchemaModifierModel> LoadSchemaTemplates(List<SchemaModifier> userConfigTemplates, 
            DatabaseMetadata databaseMetadata, AppMetadata appMetadata)
        {
            var returnVM = new ObservableCollection<SchemaModifierModel>();

            //We show every single element from the user's config. 
            var schemasToShow = userConfigTemplates;

            //We add the elements from the default metadata if not present.
            var userConfigDistinctSchemas = userConfigTemplates.Select(s => s.Name.GetSchema()).Distinct();
            var schemasToAdd = databaseMetadata.Select(s => s.Key).Distinct().Except(userConfigDistinctSchemas);
            schemasToShow.AddRange(from d in schemasToAdd select new SchemaModifier { Name = d });

            foreach (var schema in schemasToShow)
            {
                var defaultSchemaMetadata = GetSchemaMetadata(databaseMetadata, appMetadata, schema.Name);

                returnVM.Add(new SchemaModifierModel
                {
                    _name = schema.Name,
                    _templateId = schema.TemplateId,
                    _useTemplateId = schema.UseTemplateId,
                    _description = schema.Description,
                    _tables = LoadTableTemplates(schema.Tables, defaultSchemaMetadata)
                });
            }

            return returnVM;
        }

        private static ObservableCollection<TableModifierModel> LoadTableTemplates(List<TableModifier> userConfigTemplates, SchemaMetadata schemaMetadata)
        {
            var returnVM = new ObservableCollection<TableModifierModel>();

            foreach (var tableMetadata in schemaMetadata)
            {
                var tableVM = new TableModifierModel();
                tableVM._name = tableMetadata.Name;
                
                List<DataBuilder> userConfigDataBuilders = null;
                List<DerivativeSubTable> userConfigDerivativeSubTable = null;
                ForeignKeys userConfigFk = null;

                var userConfigTemplate = userConfigTemplates.FirstOrDefault(t => t.Name.ToLower() == tableMetadata.Name);
                if (userConfigTemplate != null)
                {
                    userConfigDataBuilders = userConfigTemplate.DataBuilders;

                    if (userConfigTemplate.DerativeTables != null)
                    {
                        tableVM._derativeTablesGlobalAccess = userConfigTemplate.DerativeTables.GlobalAccess;
                        tableVM._derativeTablesGlobalCascade = userConfigTemplate.DerativeTables.GlobalCascade;
                        userConfigDerivativeSubTable = userConfigTemplate.DerativeTables.DerativeSubTables;
                    }

                    if (userConfigTemplate.ForeignKeys != null)
                        userConfigFk = userConfigTemplate.ForeignKeys;

                    tableVM._isStatic = userConfigTemplate.IsStatic;
                }

                tableVM._dataBuilders = LoadDataBuildersTemplate(userConfigDataBuilders, tableMetadata.ColumnsDefinition);
                tableVM._derivativeTables = LoadDerivativeTableTemplate(userConfigDerivativeSubTable, tableMetadata.DerivativeTables);
                tableVM._foreignKeys = LoadForeingKeyTemplate(userConfigFk, tableMetadata.ForeignKeys);
                
                returnVM.Add(tableVM);
            }

            return returnVM;
        }

        private static ObservableCollection<DataBuilderModel> LoadDataBuildersTemplate(IList<DataBuilder> userConfigDataBuilders,
            IColumnDefinition[] colMetadatas)
        {
            var dbsVM = new ObservableCollection<DataBuilderModel>();

            //Load the default metadata
            foreach (var colMetadata in colMetadatas)
            {
                dbsVM.Add(new DataBuilderModel
                {
                    _builderName = colMetadata.BuilderName,
                    _columnsName = colMetadata.Name
                });
            }

            //Merge the user configuration
            if (userConfigDataBuilders != null)
            {
                foreach (var userConfigDataBuilder in userConfigDataBuilders)
                {
                    var dVM = dbsVM.FirstOrDefault(db => db.ColumnName == userConfigDataBuilder.Name);
                    if (dVM != null)
                        dVM.BuilderName = userConfigDataBuilder.BuilderName;
                }
            }

            return dbsVM;
        }

        private static ObservableCollection<DerivativeTableModifierModel> LoadDerivativeTableTemplate(IList<DerivativeSubTable> userConfigDerivativeSubTables,
            IDerivativeTable[] derivativeTableMetadatas)
        {
            var dtsVM = new ObservableCollection<DerivativeTableModifierModel>();

            //Load the default metadata
            foreach (var dtMetadata in derivativeTableMetadatas)
            {
                dtsVM.Add(new DerivativeTableModifierModel
                {
                    _access = dtMetadata.Access,
                    _cascade = dtMetadata.Cascade,
                    _database = dtMetadata.Database,
                    _schema = dtMetadata.Schema,
                    _serverId = dtMetadata.ServerId.ToString(),
                    _table = dtMetadata.Table
                });
            }

            //Merge the user configuration
            if (userConfigDerivativeSubTables != null)
            {
                foreach (var userConfigDt in userConfigDerivativeSubTables)
                {
                    var dVM = dtsVM.FirstOrDefault(dt => dt.Table == userConfigDt.Table &&
                                                         dt.Schema == userConfigDt.Schema &&
                                                         dt.Database == userConfigDt.Database &&
                                                         dt.ServerId == userConfigDt.ServerId);
                    //Modification
                    if (dVM != null)
                    {
                        dVM._access = userConfigDt.Access;
                        dVM._cascade = userConfigDt.Cascade;
                    }
                    //Ajout
                    else
                    {
                        dtsVM.Add(new DerivativeTableModifierModel
                        {
                            _access = userConfigDt.Access,
                            _cascade = userConfigDt.Cascade,
                            _database = userConfigDt.Database,
                            _schema = userConfigDt.Schema,
                            _serverId = userConfigDt.ServerId,
                            _table = userConfigDt.Table, 
                        });
                    }
                }
            }

            return dtsVM;
        }

        private static ObservableCollection<ForeignKeyModifierModel> LoadForeingKeyTemplate(ForeignKeys userConfigFks, IForeignKey[] fkMetadatas)
        {
            var fksVM = new ObservableCollection<ForeignKeyModifierModel>();

            //Load the default metadata
            foreach (var fkMetadata in fkMetadatas)
            {
                fksVM.Add(new ForeignKeyModifierModel
                {
                    _serverIdTo = fkMetadata.ServerIdTo.ToString(),
                    _databaseTo = fkMetadata.DatabaseTo,
                    _schemaTo = fkMetadata.SchemaTo,
                    _tableTo = fkMetadata.TableTo,
                    _columns = new ObservableCollection<ForeignKeyColumnModifierModel>(                    
                         (from c in fkMetadata.Columns select new ForeignKeyColumnModifierModel { _nameFrom = c.NameFrom, _nameTo = c.NameTo }))
                });
            }

            //Merge the user configuration
            if (userConfigFks != null)
            {
                //Remove
                foreach (var userConfigfk in userConfigFks.ForeignKeyRemove.Columns)
                {
                    var fks = fksVM.Where(f => f.Columns.Any(c => c.NameFrom == userConfigfk.Name));
                    foreach (var fk in fks)
                        fk.IsDeleted = true;
                }

                //Add
                foreach (var userConfigfk in userConfigFks.ForeignKeyAdd)
                {
                    var exists = fksVM.Any(f => f.ServerIdTo == userConfigfk.ServerId.ToString() &&
                                                f.DatabaseTo == userConfigfk.Database &&
                                                f.SchemaTo == userConfigfk.Schema &&
                                                f.TableTo == userConfigfk.Table &&
                                                f.Columns.Select(c => c.NameFrom + c.NameTo).Intersect(
                                                    userConfigfk.Columns.Select(c => c.NameFrom + c.NameTo)).Count() == f.Columns.Count);
                    if (!exists)
                    {
                        fksVM.Add(new ForeignKeyModifierModel
                        {
                            _serverIdTo = userConfigfk.ServerId,
                            _databaseTo = userConfigfk.Database,
                            _schemaTo = userConfigfk.Schema,
                            _tableTo = userConfigfk.Table,
                            _columns = new ObservableCollection<ForeignKeyColumnModifierModel>(
                                (from c in userConfigfk.Columns select new ForeignKeyColumnModifierModel { _nameFrom = c.NameFrom, _nameTo = c.NameTo }))
                        });
                    }
                }
            }

            return fksVM;
        }

        #endregion

        #region Commun

        /// <summary>
        /// Get the default server schema compiled from the database.
        /// For a config variable, we fetch a static defined server, database and schema from the application's metadata to show "intellisence".
        /// </summary>
        /// <param name="defaultMetadatas">Metadatas of the plain old database</param>
        /// <param name="serverId">Could be an Id like 0 or a variable like {$KEY{SERVER_VALUE}} OR {$SERVER_SOURCE{1}}.</param>
        private static ServerMetadata GetServerMetadata(AppMetadata defaultMetadatas, string serverId)
        {
            if (serverId == null)
                throw new ArgumentNullException("defaultMetadata");

            //If is a serverId
            Int16 id;
            if (Int16.TryParse(serverId, out id))
            {
                if (defaultMetadatas.ContainsKey(id))
                    return defaultMetadatas[id];
            }

            //If is a variable
            var configVar = serverId.ParseConfigVariable();
            if (configVar != null && configVar.Server != 0 && defaultMetadatas.ContainsKey(configVar.Server))
                return defaultMetadatas[configVar.Server];

            throw new ConfigurationException(String.Format("Server not found in the configuration. Id : {0}", serverId));
        }

        /// <summary>
        /// Get the default database schema compiled from the database.
        /// For a config variable, we fetch a static defined server, database and schema from the application's metadata to show "intellisence".
        /// </summary>
        /// <param name="databaseName">Could be a name like MyDb or a variable like {$KEY{SERVER_VALUE}{DATABASE_VALUE}} OR {$DATABASE_SOURCE{1}{myDb}}.</param>
        /// <param name="parentMetadata">Metadatas</param>
        /// <param name="appMetadata">App metadata</param>
        private static DatabaseMetadata GetDatabaseMetadata(ServerMetadata parentMetadata, AppMetadata appMetadata, string databaseName)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");

            databaseName = databaseName.ToLower();

            //If is a database name
            if (parentMetadata.ContainsKey(databaseName))
                return parentMetadata[databaseName];

            //If is a variable
            var v = databaseName.ParseConfigVariable();
            if (v != null && 
                appMetadata.ContainsKey(v.Server) &&
                appMetadata[v.Server].ContainsKey(v.Database) )
                 return appMetadata[v.Server][v.Database];

            throw new ConfigurationException(String.Format("Database not found in the configuration. Name : {0}", databaseName));
        }

        /// <summary>
        /// Get the default schema compiled from the database.
        /// For a config variable, we fetch a static defined server, database and schema from the application's metadata to show "intellisence".
        /// </summary>
        /// <param name="schemaName">Could be a name like DBO or a variable like {$KEY{SERVER_VALUE}{DATABASE_VALUE}{SCHEMA_VALUE}}
        ///  OR {$DATABASE_SOURCE{1}{myDb}{dbo}}.</param>
        /// <param name="databaseMetadata">Metadatas</param>
        /// <param name="appMetadata">App metadata</param>
        private static SchemaMetadata GetSchemaMetadata(DatabaseMetadata databaseMetadata, AppMetadata appMetadata, string schemaName)
        {
            if (String.IsNullOrEmpty(schemaName))
                throw new ArgumentNullException("databaseMetadata");

            schemaName = schemaName.ToLower();

            //If is a schema name
            if (databaseMetadata.ContainsKey(schemaName))
                return databaseMetadata[schemaName];

            //If is a variable
            var v = schemaName.ParseConfigVariable();
            if (v != null &&
                appMetadata.ContainsKey(v.Server) &&
                appMetadata[v.Server].ContainsKey(v.Database) &&
                appMetadata[v.Server][v.Database].ContainsKey(v.Schema))
                return appMetadata[v.Server][v.Database][v.Schema];

            throw new ConfigurationException(String.Format("Schema not found in the configuration. Name : {0}", schemaName));
        }

        #endregion
    }
}
