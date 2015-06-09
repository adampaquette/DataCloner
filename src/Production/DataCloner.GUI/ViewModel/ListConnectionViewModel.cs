using System.Collections.Generic;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;

namespace DataCloner.GUI.ViewModel
{
    public class ListConnectionViewModel : ViewModelBase
    {
        private ObservableCollection<ServerViewModel> _servers;

        public ObservableCollection<ServerViewModel> Servers
        {
            get { return _servers; }
            set { Set("Servers", ref _servers, value); }
        }

        [PreferredConstructor]
        public ListConnectionViewModel()
        {
            AddCommand = new RelayCommand<DataGrid>(g => Servers.Add(new ServerViewModel()), g => true);
            SaveCommand = new RelayCommand<ServerViewModel>(Save, s => true);

            if (IsInDesignMode)
            {
                Servers = new ObservableCollection<ServerViewModel>
                {
                    new ServerViewModel(),
                    new ServerViewModel()
                };
            }
        }

        public ListConnectionViewModel(IEnumerable<Connection> Connections) : base()
        {
            _servers = new ObservableCollection<ServerViewModel>();

            foreach (var conn in Connections)
                _servers.Add(new ServerViewModel(conn));
        }

        public RelayCommand<DataGrid> AddCommand { get; private set; }
        public RelayCommand<ServerViewModel> SaveCommand { get; private set; }

        public void Save(ServerViewModel current)
        {
            var config = Configuration.Load(Configuration.ConfigFileName);
            var app = config.Applications.FirstOrDefault(a => a.Id == Settings.Default.DefaultAppId);
            if (app == null)
                app = config.Applications.First();

            app.ConnectionStrings = new List<Connection>();
            foreach (var s in Servers)
            {
                app.ConnectionStrings.Add(new Connection
                {
                    ConnectionString = s.ConnectionString,
                    Id = s.Id,
                    Name = s.Name,
                    ProviderName = s.ProviderName
                });
            }

            config.Save(Configuration.ConfigFileName);
        }
    }
}
