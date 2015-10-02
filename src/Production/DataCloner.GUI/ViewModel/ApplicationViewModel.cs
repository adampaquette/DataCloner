﻿using Cache = DataCloner.DataClasse.Cache;
using DataCloner.DataAccess;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;
using DataCloner.GUI.Services;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataCloner.Framework;
using System.Collections.ObjectModel;
using DataCloner.GUI.Model;

namespace DataCloner.GUI.ViewModel
{
    class ApplicationViewModel : ValidatableModel
    {
        private Int16 _id;
        private string _name;
        private ListConnectionViewModel _connections;
        private TemplatesViewModel _templates;
        private bool _isValid = true;
        private Cache.Cache _defaultSchema;

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

        public ApplicationViewModel(Application app)
        {
            SaveCommand = new RelayCommand(Save, () => IsValid);

            Cache.Cache.InitializeSchema(new QueryDispatcher(), app, ref _defaultSchema);

            ConfigurationService.Load(this, app, _defaultSchema);
        }

        public RelayCommand SaveCommand { get; private set; }

        private void Save()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("You must not call Save when CanSave returns false.");
            }

            ConfigurationService.Save(this, _defaultSchema);
        }
    }
}
