using DataCloner.DataClasse.Configuration;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.ViewModel
{
    class ApplicationViewModel : AnnotationViewModelBase
    {
        public Int16 _id;
        public string _name;
        public ListConnectionViewModel _connections;

        [Required]
        public Int16 Id
        {
            get { return _id; }
            set { Set("Id", ref _id, value); }
        }

        [Required]
        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        [Required]
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
