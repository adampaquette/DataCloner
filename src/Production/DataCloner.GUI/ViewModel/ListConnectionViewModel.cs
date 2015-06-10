using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Properties;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;

namespace DataCloner.GUI.ViewModel
{
    class ListConnectionViewModel : AnnotationViewModelBase
    {
        private ObservableCollection<ConnectionViewModel> _connections;

        public ObservableCollection<ConnectionViewModel> Connections
        {
            get { return _connections; }
            set { Set("Connections", ref _connections, value); }
        }

        [PreferredConstructor]
        public ListConnectionViewModel()
        {
            AddCommand = new RelayCommand<DataGrid>(g => Connections.Add(new ConnectionViewModel()), g => true);
            SaveCommand = new RelayCommand<ConnectionViewModel>(Save, s => true);

            if (IsInDesignMode)
            {
                Connections = new ObservableCollection<ConnectionViewModel>
                {
                    new ConnectionViewModel(),
                    new ConnectionViewModel()
                };
            }
        }

        public ListConnectionViewModel(IEnumerable<Connection> Connections) : base()
        {
            _connections = new ObservableCollection<ConnectionViewModel>();

            foreach (var conn in Connections)
                _connections.Add(new ConnectionViewModel(conn));
        }

        public RelayCommand<DataGrid> AddCommand { get; private set; }
        public RelayCommand<ConnectionViewModel> SaveCommand { get; private set; }

        public void Save(ConnectionViewModel current)
        {
            var config = Configuration.Load(Configuration.ConfigFileName);
            var app = config.Applications.FirstOrDefault(a => a.Id == Settings.Default.DefaultAppId);
            if (app == null)
                app = config.Applications.First();

            app.ConnectionStrings = new List<Connection>();
            foreach (var s in Connections)
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
