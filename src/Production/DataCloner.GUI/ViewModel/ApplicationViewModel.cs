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
        public bool _isValid = true;

        [Required]
        public Int16 Id
        {
            get { return _id; }
            set { ValidateAndSet(ref _id, value); }
        }

        [Required]
        public string Name
        {
            get { return _name; }
            set { ValidateAndSet(ref _name, value); }
        }

        [Required]
        public ListConnectionViewModel Connections
        {
            get { return _connections; }
            set { ValidateAndSet(ref _connections, value); }
        }

        public bool IsValid
        {
            get { return _isValid; }
            set { Set(ref _isValid, value); }
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
