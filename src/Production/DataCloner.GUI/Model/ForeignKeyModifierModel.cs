using DataCloner.Metadata;
using DataCloner.GUI.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;


namespace DataCloner.GUI.Model
{
    class ForeignKeyModifierModel : ValidatableModel
    {
        private string _serverIdTo;
        private string _databaseTo;
        private string _schemaTo;
        private string _tableTo;
        private bool _isDeleted;
        private ObservableCollection<ForeignKeyColumnModifierModel> _columns;


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

        public ForeignKeyModifierModel(IForeignKey fk)
        {
            _serverIdTo = fk.ServerIdTo.ToString();
            _databaseTo = fk.DatabaseTo;
            _schemaTo = fk.SchemaTo;
            _tableTo = fk.TableTo;

            _columns = new ObservableCollection<ForeignKeyColumnModifierModel>();
            foreach (var col in fk.Columns)
                _columns.Add(new ForeignKeyColumnModifierModel(col));
        }
    }
}
