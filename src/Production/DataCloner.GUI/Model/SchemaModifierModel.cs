using DataCloner.DataClasse.Cache;
using DataCloner.GUI.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class SchemaModifierModel : ValidatableModel
    {
        private string _name;
        private Int16 _templateId;
        private Int16 _useTemplateId;
        private ObservableCollection<TableModifierModel> _tables;

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

        public ObservableCollection<TableModifierModel> Tables
        {
            get { return _tables; }
            set { SetProperty(ref _tables, value); }
        }

        public SchemaModifierModel()
        {      
            //Pour que le binding puisse créer une nouvelle ligne
        }

        public SchemaModifierModel(KeyValuePair<string, TableSchema[]> defaultSchema)
        {
            _name = defaultSchema.Key.ToString();

            _tables = new ObservableCollection<TableModifierModel>();
            foreach (var table in defaultSchema.Value)
                _tables.Add(new TableModifierModel(table));
        }
    }
}
