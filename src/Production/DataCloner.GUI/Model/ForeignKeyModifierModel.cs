using DataCloner.Metadata;
using DataCloner.GUI.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;


namespace DataCloner.GUI.Model
{
    class ForeignKeyModifierModel : ValidatableModelBase
    {
        internal string _serverIdTo;
        internal string _databaseTo;
        internal string _schemaTo;
        internal string _tableTo;
        internal ObservableCollection<ForeignKeyColumnModifierModel> _columns;

        private bool _isDeleted;

        [Required]
        public string ServerIdTo
        {
            get { return _serverIdTo; }
            set { SetPropertyAndValidate(ref _serverIdTo, value); }
        }

        [Required]
        public string DatabaseTo
        {
            get { return _databaseTo; }
            set { SetPropertyAndValidate(ref _databaseTo, value); }
        }

        [Required]
        public string SchemaTo
        {
            get { return _schemaTo; }
            set { SetPropertyAndValidate(ref _schemaTo, value); }
        }

        [Required]
        public string TableTo
        {
            get { return _tableTo; }
            set { SetPropertyAndValidate(ref _tableTo, value); }
        }

        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { SetProperty(ref _isDeleted, value); }
        }

        public ObservableCollection<ForeignKeyColumnModifierModel> Columns
        {
            get { return _columns; }
            set { SetProperty(ref _columns, value); }
        }

        public ForeignKeyModifierModel()
        {
            //Pour que le binding puisse créer une nouvelle ligne
            _columns = new ObservableCollection<ForeignKeyColumnModifierModel>();
        }
    }
}
