using DataCloner.GUI.Framework;
using DataCloner.GUI.Services;
using DataCloner.Metadata;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.ViewModel
{
    class ApplicationViewModel : ValidatableModel
    {
        internal Int16 _id;
        internal string _name;
        internal ListConnectionViewModel _connections;
        internal TemplatesViewModel _templates;
        internal AppMetadata _defaultMetadatas;
        
        private bool _isValid = true;

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

        public ListConnectionViewModel Connections
        {
            get { return _connections; }
            set { SetPropertyAndValidate(ref _connections, value); }
        }

        public TemplatesViewModel Templates
        {
            get { return _templates; }
            set { SetPropertyAndValidate(ref _templates, value); }
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

        public ApplicationViewModel()
        {
            SaveCommand = new RelayCommand(Save, () => IsValid);
        }

        public RelayCommand SaveCommand { get; private set; }

        private void Save()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("You must not call Save when CanSave returns false.");
            }

            this.Save(_defaultMetadatas, "northWind.dcproj");
        }
    }
}
