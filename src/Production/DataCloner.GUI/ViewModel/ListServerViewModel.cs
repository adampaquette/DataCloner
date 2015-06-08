using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace DataCloner.GUI.ViewModel
{
    public class ListServerViewModel : ViewModelBase
    {
        private ObservableCollection<ServerViewModel> _connections;

        public ObservableCollection<ServerViewModel> Connections
        {
            get { return _connections; }
            set { Set("Connections", ref _connections, value); }
        }

        public ListServerViewModel()
        {
            Connections = new ObservableCollection<ServerViewModel>();

            if (IsInDesignMode)
            {
                Connections = new ObservableCollection<ServerViewModel>
                {
                    new ServerViewModel(),
                    new ServerViewModel()
                };
            }
        }
    }
}
