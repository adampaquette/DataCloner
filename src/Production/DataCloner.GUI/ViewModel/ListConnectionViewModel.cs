﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;

namespace DataCloner.GUI.ViewModel
{
    class ListConnectionViewModel : ValidatableModel
    {
        private ObservableCollection<ConnectionViewModel> _connections;

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

        public ListConnectionViewModel(IEnumerable<Connection> Connections) : base()
        {
            _connections = new ObservableCollection<ConnectionViewModel>();

            foreach (var conn in Connections)
                _connections.Add(new ConnectionViewModel(conn));
        }
    }
}
