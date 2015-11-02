using DataCloner.GUI.Framework;
using DataCloner.GUI.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataCloner.GUI.ViewModel
{
    class TemplatesViewModel : ValidatableModelBase
    {
        private static readonly IList<AccessWithDescription> _accessWithDescriptions;

        private ServerModifierModel _selectedServer;
        private DatabaseModifierModel _selectedDatabase;
        private SchemaModifierModel _selectedSchema;
        private TableModifierModel _selectedTable;
        private ForeignKeyModifierModel _selectedForeignKey;
        
        private bool _isDatabaseTopLevel;
        private bool _isSchemaTopLevel;

        internal ObservableCollection<ServerModifierModel> _serverModifiers;
        internal ObservableCollection<DatabaseModifierModel> _databaseModifiers;
        internal ObservableCollection<SchemaModifierModel> _schemaModifiers;

        public class AccessWithDescription
        {
            public DerivativeTableAccess Key { get; set; }
            public string Description { get; set; }
        }

        public IList<AccessWithDescription> AccessWithDescriptions
        {
            get { return _accessWithDescriptions; }
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

        static TemplatesViewModel()
        {
            _accessWithDescriptions = new List<AccessWithDescription>
            {
                new AccessWithDescription { Key = DerivativeTableAccess.NotSet, Description = "Non déféni" },
                new AccessWithDescription { Key = DerivativeTableAccess.Denied, Description = "Refusé" },
                new AccessWithDescription { Key = DerivativeTableAccess.Forced, Description = "Forcé" }
            };
        }
    }
}
