using Cache = DataCloner.DataClasse.Cache;
using DataCloner.DataAccess;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataCloner.Framework;

namespace DataCloner.GUI.ViewModel
{
    class ApplicationViewModel : ValidatableModel
    {
        private Int16 _id;
        private string _name;
        private ListConnectionViewModel _connections;
        private TemplatesViewModel _templates;
        private bool _isValid = true;
        private Cache.Cache _defaultSchema;

        [Required]
        public Int16 Id
        {
            get { return _id; }
            set { SetPropertyAndValidate(ref _id, value); }
        }

        [Required]
        public string Name
        {
            get { return _name; }
            set { SetPropertyAndValidate(ref _name, value); }
        }

        public ListConnectionViewModel Connections
        {
            get { return _connections; }
            set { SetPropertyAndValidate(ref _connections, value); }
        }

        public TemplatesViewModel Templates
        {
            get { return _templates; }
            set { SetPropertyAndValidate(ref _templates, value); }
        }

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                SetProperty(ref _isValid, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public ApplicationViewModel(Application app)
        {
            SaveCommand = new RelayCommand(Save, () => IsValid);

            Cache.Cache.InitializeSchema(new QueryDispatcher(), app, ref _defaultSchema);

            _id = app.Id;
            _name = app.Name;
            _connections = new ListConnectionViewModel(app.ConnectionStrings);
            _templates = new TemplatesViewModel(app.ModifiersTemplates, app.ConnectionStrings, _defaultSchema);
        }

        public RelayCommand SaveCommand { get; private set; }

        private void Save()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("You must not call Save when CanSave returns false.");
            }

            var config = Configuration.Load(Configuration.ConfigFileName);
            var app = config.Applications.FirstOrDefault(a => a.Id == Id);
            app.Id = Id;
            app.Name = Name;

            UpdateConnectionStrings(app);
            UpdateTemplates(app.ModifiersTemplates);

            config.Save(Configuration.ConfigFileName);
        }

        private void UpdateConnectionStrings(Application app)
        {
            app.ConnectionStrings = new List<Connection>();
            foreach (var conn in Connections.Connections)
            {
                app.ConnectionStrings.Add(new Connection
                {
                    Id = conn.Id,
                    Name = conn.Name,
                    ProviderName = conn.ProviderName,
                    ConnectionString = conn.ConnectionString
                });
            }
        }

        private void UpdateTemplates(Modifiers userConfigModifiers)
        {
            foreach (var mergedServer in _templates.ServerModifiers)
            {
                var defaultServerSchema = GetServerSchema(mergedServer.Id);
                MergeServer(mergedServer, userConfigModifiers.ServerModifiers, defaultServerSchema);
            }
        }

        private bool MergeServer(Model.ServerModifierModel mergedServer,
                                 List<ServerModifier> userConfigServers,
                                 Dictionary<string, Dictionary<string, Cache.TableSchema[]>> defaultServerSchema)
        {
            var hasChange = false;
            var userConfigServer = userConfigServers.FirstOrDefault(s => s.Id == mergedServer.Id);

            if (defaultServerSchema == null)
                hasChange = true;

            //Add new
            if (userConfigServer == null)
            {
                userConfigServer = new ServerModifier
                {
                    Id = mergedServer.Id,
                    TemplateId = mergedServer.TemplateId,
                    UseTemplateId = mergedServer.UseTemplateId
                };
                userConfigServers.Add(userConfigServer);
            }

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

        private bool MergeDatabase(Model.DatabaseModifierModel mergedDatabase,
                                   List<DatabaseModifier> userConfigDatabases,
                                   Dictionary<string, Cache.TableSchema[]> defaultDatabaseSchema)
        {
            var hasChange = false;
            var userConfigDatabase = userConfigDatabases.FirstOrDefault(d => d.Name == mergedDatabase.Name);

            if (defaultDatabaseSchema == null)
                hasChange = true;

            //Add new
            if (userConfigDatabase == null)
            {
                userConfigDatabase = new DatabaseModifier
                {
                    Name = mergedDatabase.Name,
                    TemplateId = mergedDatabase.TemplateId,
                    UseTemplateId = mergedDatabase.UseTemplateId
                };
                userConfigDatabases.Add(userConfigDatabase);
            }

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

        private bool MergeSchema(Model.SchemaModifierModel mergedSchema,
                                List<SchemaModifier> userConfigSchemas,
                                Cache.TableSchema[] defaultSchema)
        {
            var hasChange = false;
            var userConfigSchema = userConfigSchemas.FirstOrDefault(s => s.Name == mergedSchema.Name);

            //Add new
            if (userConfigSchema == null)
            {
                userConfigSchema = new SchemaModifier
                {
                    Name = mergedSchema.Name,
                    TemplateId = mergedSchema.TemplateId,
                    UseTemplateId = mergedSchema.UseTemplateId
                };
                userConfigSchemas.Add(userConfigSchema);
            }

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

        private bool MergeTable(Model.TableModifierModel mergedTable,
                                List<TableModifier> userConfigTables,
                                Cache.TableSchema defaultTable)
        {
            var hasChange = false;
            var userConfigTable = userConfigTables.FirstOrDefault(t => t.Name == mergedTable.Name);

            //Add new
            if (userConfigTable == null)
            {
                userConfigTables.Add(new TableModifier
                {
                    Name = mergedTable.Name,
                    IsStatic = mergedTable.IsStatic,
                    DataBuilders = null,
                    DerativeTables = null,
                    ForeignKeys = null
                });

                hasChange = true;
            }
            else
            {

            }

            return hasChange;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverId">Could be an Id like 0 or a variable like {$KEY{VALUE}} OR {$SERVER_SOURCE{1}}.</param>
        /// <returns></returns>
        private Dictionary<string, Dictionary<string, Cache.TableSchema[]>> GetServerSchema(string serverId)
        {
            Int16 id;

            if (serverId == null) return null;

            //If is a serverId
            if (Int16.TryParse(serverId, out id))
            {
                if (_defaultSchema.DatabasesSchema.ContainsKey(id))
                    return _defaultSchema.DatabasesSchema[id];
            }

            //If is a variable
            id = serverId.ExtractVariableValueInt16();
            if (id != 0 && _defaultSchema.DatabasesSchema.ContainsKey(id))
                return _defaultSchema.DatabasesSchema[id];

            return null;
        }

        private Dictionary<string, Cache.TableSchema[]> GetDatabaseSchema(string databaseName,
            Dictionary<string, Dictionary<string, Cache.TableSchema[]>> serverSchema)
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

        private Cache.TableSchema[] GetSchema(string schemaName,
            Dictionary<string, Cache.TableSchema[]> databaseSchema)
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
    }
}
