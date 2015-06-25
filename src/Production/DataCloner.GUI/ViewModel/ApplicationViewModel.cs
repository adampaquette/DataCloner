using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataCloner.GUI.ViewModel
{
    class ApplicationViewModel : ValidatableModel
    {
        public Int16 _id;
        public string _name;
        public ListConnectionViewModel _connections;
        public bool _isValid = true;

        [Required]
        public Int16 Id
        {
            get { return _id; }
            set { SetPropertyAndValidate(ref _id, value); }
        }

        [Required]
        public string Name
        {
            get { return _name; }
            set { SetPropertyAndValidate(ref _name, value); }
        }

        [Required]
        public ListConnectionViewModel Connections
        {
            get { return _connections; }
            set { SetPropertyAndValidate(ref _connections, value); }
        }

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                SetProperty(ref _isValid, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        [PreferredConstructor]
        public ApplicationViewModel()
        {
            SaveCommand = new RelayCommand(Save, () => IsValid);
        }

        public ApplicationViewModel(Application app) : this()
        {
            _id = app.Id;
            _name = app.Name;
            _connections = new ListConnectionViewModel(app.ConnectionStrings);
        }

        public RelayCommand SaveCommand { get; private set; }

        private void Save()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("You must not call Save when CanSave returns false.");
            }

            var config = Configuration.Load(Configuration.ConfigFileName);
            var app = config.Applications.FirstOrDefault(a => a.Id == Id);
            app.Id = Id;
            app.Name = Name;
            app.ConnectionStrings = new List<Connection>();

            foreach (var conn in Connections.Connections)
            {
                app.ConnectionStrings.Add(new Connection
                {
                    Id = conn.Id,
                    Name = conn.Name,
                    ProviderName = conn.ProviderName,
                    ConnectionString = conn.ConnectionString
                });
            }

            config.Save(Configuration.ConfigFileName);
        }
    }
}
