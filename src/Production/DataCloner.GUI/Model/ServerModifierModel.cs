using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class ServerModifierModel : ValidatableModel
    {
        private string _id;
        private Int32 _templateId;
        private Int32 _useTemplateId;
        private Int16 _basedOnServerId;
        private ObservableCollection<DatabaseModifierModel> _databases;

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

        [Required]
        public Int16 BasedOnServerId
        {
            get { return _basedOnServerId; }
            set { SetPropertyAndValidate(ref _basedOnServerId, value); }
        }

        public ObservableCollection<DatabaseModifierModel> Databases
        {
            get { return _databases; }
            set { SetProperty(ref _databases, value); }
        }

        public ServerModifierModel()
        {
        }

        public ServerModifierModel(ServerModifier server)
        {
            _id = server.Id;
            _templateId = server.TemplateId;
            _useTemplateId = server.UseTemplateId;
            _basedOnServerId = server.BasedOnServerId;

            Databases = new ObservableCollection<DatabaseModifierModel>();
            foreach (var database in server.Databases)
                Databases.Add(new DatabaseModifierModel(database));
        }
    }
}
