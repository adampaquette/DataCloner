using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class DatabaseModifierModel : ValidatableModel
    {
        private string _name;
        private Int16 _templateId;
        private Int16 _useTemplateId;
        private ObservableCollection<SchemaModifierModel> _schemas;

        [Required]
        public string Name
        {
            get { return _name; }
            set { SetPropertyAndValidate(ref _name, value); }
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

        public DatabaseModifierModel(KeyValuePair<string, Dictionary<string, TableSchema[]>> defaultSchema)
        {
            _name = defaultSchema.Key.ToString();

            _schemas = new ObservableCollection<SchemaModifierModel>();
            foreach (var schema in defaultSchema.Value)
                _schemas.Add(new SchemaModifierModel(schema));
        }
    }
}
