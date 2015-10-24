using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataCloner.Configuration;
using DataCloner.GUI.Framework;

namespace DataCloner.GUI.ViewModel
{
    class ListConnectionViewModel : ValidatableModel
    {
        internal ObservableCollection<ConnectionViewModel> _connections;

        public ObservableCollection<ConnectionViewModel> Connections
        {
            get { return _connections; }
            set { SetPropertyAndValidate(ref _connections, value); }
        }

        public ListConnectionViewModel()
        {
            //if (IsInDesignMode)
            //{
            //    Connections = new ObservableCollection<ConnectionViewModel>
            //    {
            //        new ConnectionViewModel(),
            //        new ConnectionViewModel()
            //    };
            //}
        }
    }
}
