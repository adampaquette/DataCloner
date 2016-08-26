using DataCloner.Core.Metadata;
using DataCloner.GUI.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class DatabaseModifierModel : ValidatableModelBase
    {
        internal string _name;
        internal string _description;
        internal Int16 _templateId;
        internal Int16 _useTemplateId;
        internal ObservableCollection<SchemaModifierModel> _schemas;

        [Required]
        public string Name
        {
            get { return _name; }
            set { SetPropertyAndValidate(ref _name, value); }
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

        public ObservableCollection<SchemaModifierModel> Schemas
        {
            get { return _schemas; }
            set { SetProperty(ref _schemas, value); }
        }

        public DatabaseModifierModel()
        {
            //Pour que le binding puisse créer une nouvelle ligne
        }

        public DatabaseModifierModel(KeyValuePair<string, Dictionary<string, TableMetadata[]>> defaultSchema)
        {
            _name = defaultSchema.Key.ToString();

            _schemas = new ObservableCollection<SchemaModifierModel>();
            foreach (var schema in defaultSchema.Value)
                _schemas.Add(new SchemaModifierModel(schema));
        }
    }
}
