using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.GUI.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class ServerModifierModel : ValidatableModel
    {
        private string _id;
        private Int16 _templateId;
        private Int16 _useTemplateId;
        private ObservableCollection<DatabaseModifierModel> _databases;

        [Required]
        public string Id
        {
            get { return _id; }
            set { SetPropertyAndValidate(ref _id, value); }
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

        //public ServerModifierModel(ServerModifier server)
        //{
        //    _id = server.Id;
        //    _templateId = server.TemplateId;
        //    _useTemplateId = server.UseTemplateId;
        //    _basedOnServerId = server.BasedOnServerId;

        //    Databases = new ObservableCollection<DatabaseModifierModel>();
        //    foreach (var database in server.Databases)
        //        Databases.Add(new DatabaseModifierModel(database));
        //}

        public ServerModifierModel(KeyValuePair<Int16, Dictionary<string, Dictionary<string, TableSchema[]>>> defaultSchema)
        {
            _id = defaultSchema.Key.ToString();

            _databases = new ObservableCollection<DatabaseModifierModel>();
            foreach (var database in defaultSchema.Value)
                _databases.Add(new DatabaseModifierModel(database));
        }
    }
}
