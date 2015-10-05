using Cache = DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;
using DataCloner.GUI.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataCloner.GUI.ViewModel
{
    class TemplatesViewModel : ValidatableModel
    {
        private Modifiers _userConfigTemplates;
        private bool _isDatabaseTopLevel;
        private bool _isSchemaTopLevel;
        private ObservableCollection<ServerModifierModel> _serverModifiers;
        private ServerModifierModel _selectedServer;
        private DatabaseModifierModel _selectedDatabase;
        private SchemaModifierModel _selectedSchema;
        private TableModifierModel _selectedTable;
        private ForeignKeyModifierModel _selectedForeignKey;
        private Cache.DatabasesSchema _defaultSchema;
        private ObservableCollection<AccessWithDescription> _accessWithDescriptions;

        public class AccessWithDescription
        {
            public DerivativeTableAccess Key { get; set; }
            public string Description { get; set; }
        }

        public ObservableCollection<AccessWithDescription> AccessWithDescriptions
        {
            get { return _accessWithDescriptions; }
            set { _accessWithDescriptions = value; }
        }

        public bool IsDatabaseTopLevel
        {
            get { return _isDatabaseTopLevel; }
            set
            {
                SetProperty(ref _isDatabaseTopLevel, value);
                RaisePropertyChanged("ShouldHideServers");
            }
        }

        public bool IsSchemaTopLevel
        {
            get { return _isSchemaTopLevel; }
            set
            {
                SetProperty(ref _isSchemaTopLevel, value);
                RaisePropertyChanged("ShouldHideServers");
                RaisePropertyChanged("ShouldHideDatabases");
            }
        }

        public ObservableCollection<ServerModifierModel> ServerModifiers
        {
            get { return _serverModifiers; }
            set { SetPropertyAndValidate(ref _serverModifiers, value); }
        }

        public bool ShouldHideServers { get { return IsSchemaTopLevel || IsDatabaseTopLevel; } }

        public bool ShouldHideDatabases { get { return IsSchemaTopLevel; } }

        public ServerModifierModel SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                SetProperty(ref _selectedServer, value);
            }
        }

        public DatabaseModifierModel SelectedDatabase
        {
            get { return _selectedDatabase; }
            set
            {
                SetProperty(ref _selectedDatabase, value);
            }
        }

        public SchemaModifierModel SelectedSchema
        {
            get { return _selectedSchema; }
            set
            {
                SetProperty(ref _selectedSchema, value);
            }
        }

        public TableModifierModel SelectedTable
        {
            get { return _selectedTable; }
            set
            {
                SetProperty(ref _selectedTable, value);
            }
        }

        public ForeignKeyModifierModel SelectedForeignKey
        {
            get { return _selectedForeignKey; }
            set
            {
                SetProperty(ref _selectedForeignKey, value);
            }
        }

        public TemplatesViewModel(Modifiers modifiersTemplates, Cache.DatabasesSchema defaultSchema)
        {
            _serverModifiers = new ObservableCollection<ServerModifierModel>();
            _userConfigTemplates = modifiersTemplates;
            _defaultSchema = defaultSchema;

            _accessWithDescriptions = new ObservableCollection<AccessWithDescription>
            {
                new AccessWithDescription { Key = DerivativeTableAccess.NotSet, Description = "Non déféni" },
                new AccessWithDescription { Key = DerivativeTableAccess.Denied, Description = "Refusé" },
                new AccessWithDescription { Key = DerivativeTableAccess.Forced, Description = "Forcé" }
            };

            //Step 1 : Add default serveurs
            foreach (var server in defaultSchema)
            {
                _serverModifiers.Add(new ServerModifierModel(server));
            }

            //foreach (var server in modifiers.ServerModifiers)
            //    _serverModifiers.Add(new ServerModifierModel(server));
        }
    }
}
