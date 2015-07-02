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
        private ObservableCollection<Connection> _connections;


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
            set { SetProperty(ref _selectedServer, value); }
        }

        public ObservableCollection<Connection> Connections { get { return _connections; } }

        public TemplatesViewModel(Modifiers modifiers, List<Connection> conns)
        {
            _serverModifiers = new ObservableCollection<ServerModifierModel>();
            _connections = new ObservableCollection<Connection>();
            _userConfigTemplates = modifiers;

            _connections.Add(new Connection { Id = 0, Name = "" });
            foreach (var conn in conns)
                _connections.Add(conn);

            foreach (var server in modifiers.ServerModifiers)
                _serverModifiers.Add(new ServerModifierModel(server));
        }
    }
}
