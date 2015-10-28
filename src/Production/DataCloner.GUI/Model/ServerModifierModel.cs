using DataCloner.Metadata;
using DataCloner.Configuration;
using DataCloner.GUI.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class ServerModifierModel : ValidatableModel
    {
        internal string _id;
        internal string _description;
        internal Int16 _templateId;
        internal Int16 _useTemplateId;
        internal ObservableCollection<DatabaseModifierModel> _databases;

        [Required]
        public string Id
        {
            get { return _id; }
            set { SetPropertyAndValidate(ref _id, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetPropertyAndValidate(ref _description, value); }
        }

        [Required]
        public Int16 TemplateId
        {
            get { return _templateId; }
            set { SetPropertyAndValidate(ref _templateId, value); }
        }

        [Required]
        public Int16 UseTemplateId
        {
            get { return _useTemplateId; }
            set { SetPropertyAndValidate(ref _useTemplateId, value); }
        }

        public ObservableCollection<DatabaseModifierModel> Databases
        {
            get { return _databases; }
            set { SetProperty(ref _databases, value); }
        }

        public ServerModifierModel()
        {
            //Pour que le binding puisse créer une nouvelle ligne
        }

        public ServerModifierModel(KeyValuePair<Int16, Dictionary<string, Dictionary<string, TableMetadata[]>>> defaultSchema)
        {
            _id = defaultSchema.Key.ToString();

            _databases = new ObservableCollection<DatabaseModifierModel>();
            foreach (var database in defaultSchema.Value)
                _databases.Add(new DatabaseModifierModel(database));
        }
    }
}
