using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class DatabaseModifierModel : ValidatableModel
    {
        private string _name;
        private Int32 _templateId;
        private Int32 _useTemplateId;

        [Required]
        public string Name
        {
            get { return _name; }
            set { SetPropertyAndValidate(ref _name, value); }
        }

        [Required]
        public Int32 TemplateId
        {
            get { return _templateId; }
            set { SetPropertyAndValidate(ref _templateId, value); }
        }

        [Required]
        public Int32 UseTemplateId
        {
            get { return _useTemplateId; }
            set { SetPropertyAndValidate(ref _useTemplateId, value); }
        }

        public DatabaseModifierModel()
        {
        }

        public DatabaseModifierModel(DatabaseModifier database)
        {
            _name = database.Name;
            _templateId = database.TemplateId;
            _useTemplateId = database.UseTemplateId;
        }
    }
}
