using System.Linq;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Message;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;

namespace DataCloner.GUI.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private Configuration _config;
        private ApplicationViewModel _currentApp;

        public MainViewModel()
        {
            _config = Configuration.Load(Configuration.ConfigFileName);
            var app = _config.Applications.FirstOrDefault(a => a.Id == Settings.Default.DefaultAppId);
            if (app == null)
                app = _config.Applications.FirstOrDefault();
            _currentApp = new ApplicationViewModel(app);

            SaveCommand = new RelayCommand(Save, () => _currentApp.IsValid);
        }

        public ApplicationViewModel CurrentApp
        {
            get { return _currentApp; }
            set
            {
                if (Set(ref _currentApp, value))
                {
                    Settings.Default.DefaultAppId = _currentApp.Id;
                    RaisePropertyChanged("ApplicationName");

                    MessengerInstance.Send(new SelectedApplicationMessage { Application = _currentApp });
                }
            }
        }

        public string ApplicationName { get { return _currentApp?.Name; } }

        public RelayCommand SaveCommand { get; private set; }

        private void Save()
        {
            var config = Configuration.Load(Configuration.ConfigFileName);
            var app = config.Applications.FirstOrDefault(a => a.Id == _currentApp.Id);          
            app.Id = _currentApp.Id;
            app.Name = _currentApp.Name;
            app.ConnectionStrings = new List<Connection>();

            foreach (var conn in _currentApp.Connections.Connections)
            {
                app.ConnectionStrings.Add(new Connection
                {
                    Id = conn.Id,
                    Name = conn.Name,
                    ProviderName = conn.ProviderName,
                    ConnectionString = conn.ConnectionString
                });
            }

            config.Save(Configuration.ConfigFileName);
        }
    }
}