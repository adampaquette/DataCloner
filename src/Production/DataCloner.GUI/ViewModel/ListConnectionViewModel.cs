using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataCloner.DataClasse.Configuration;
using GalaSoft.MvvmLight.Ioc;

namespace DataCloner.GUI.ViewModel
{
    class ListConnectionViewModel : AnnotationViewModelBase
    {
        private ObservableCollection<ConnectionViewModel> _connections;

        public ObservableCollection<ConnectionViewModel> Connections
        {
            get { return _connections; }
            set { Set(ref _connections, value); }
        }

        [PreferredConstructor]
        public ListConnectionViewModel()
        {
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
    }
}
