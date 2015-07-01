using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class ServerModifierModel : ValidatableModel
    {
        private string _id;
        private Int32 _templateId;
        private Int32 _useTemplateId;

        [Required]
        public string Id
        {
            get { return _id; }
            set { SetPropertyAndValidate(ref _id, value); }
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

        public ServerModifierModel(ServerModifier server)
        {
            _id = server.Id;
            _templateId = server.TemplateId;
            _useTemplateId = server.UseTemplateId;
        }
    }
}
