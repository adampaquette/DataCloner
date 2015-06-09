using System.Collections.Generic;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight.CommandWpf;

namespace DataCloner.GUI.ViewModel
{
    public class ListServerViewModel : ViewModelBase
    {
        private ObservableCollection<ServerViewModel> _servers;

        public ObservableCollection<ServerViewModel> Servers
        {
            get { return _servers; }
            set { Set("Servers", ref _servers, value); }
        }

        public ListServerViewModel()
        {
            Servers = new ObservableCollection<ServerViewModel>();

            var config = Configuration.Load(Configuration.ConfigFileName);
            var app = config.Applications.FirstOrDefault(a => a.Id == Settings.Default.DefaultAppId);
            if (app == null)
                app = config.Applications.First();
            foreach (var s in app.ConnectionStrings)
            {
                Servers.Add(new ServerViewModel
                {
                    ConnectionString = s.ConnectionString,
                    Id = s.Id,
                    Name = s.Name,
                    ProviderName = s.ProviderName
                });
            }

            AddCommand = new RelayCommand<DataGrid>((g) => Servers.Add(new ServerViewModel()),
                                                    (g) => true);
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
