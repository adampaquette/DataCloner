using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCloner.DataClasse.Configuration;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

namespace DataCloner.GUI.ViewModel
{
    public class ApplicationViewModel : ViewModelBase
    {
        public Int16 _id;
        public string _name;
        public ListConnectionViewModel _connections;

        public Int16 Id
        {
            get { return _id; }
            set { Set("Id", ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        public ListConnectionViewModel Connections
        {
            get { return _connections; }
            set { Set("Connections", ref _connections, value); }
        }

        [PreferredConstructor]
        public ApplicationViewModel()
        {
        }

        public ApplicationViewModel(Application app) : base()
        {
            _id = app.Id;
            _name = app.Name;
            _connections = new ListConnectionViewModel(app.ConnectionStrings);
        }
    }
}
