using DataCloner.Metadata;
using DataCloner.GUI.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class TableModifierModel : ValidatableModel
    {
        internal string _name;
        internal bool _isStatic;
        internal ObservableCollection<ForeignKeyModifierModel> _foreignKeys;
        internal ObservableCollection<DerivativeTableModifierModel> _derivativeTables;
        internal ObservableCollection<DataBuilderModel> _dataBuilders;
        internal DerivativeTableAccess _derativeTablesGlobalAccess;
        internal bool _derativeTablesGlobalCascade;

        [Required]
        public string Name
        {
            get { return _name; }
            set { SetPropertyAndValidate(ref _name, value); }
        }

        [Required]
        public bool IsStatic
        {
            get { return _isStatic; }
            set { SetPropertyAndValidate(ref _isStatic, value); }
        }

        public ObservableCollection<ForeignKeyModifierModel> ForeignKeys
        {
            get { return _foreignKeys; }
            set { SetProperty(ref _foreignKeys, value); }
        }

        public ObservableCollection<DerivativeTableModifierModel> DerivativeTables
        {
            get { return _derivativeTables; }
            set { SetProperty(ref _derivativeTables, value); }
        }

        public ObservableCollection<DataBuilderModel> DataBuilders
        {
            get { return _dataBuilders; }
            set { SetProperty(ref _dataBuilders, value); }
        }

        [Required]
        public DerivativeTableAccess DerativeTablesGlobalAccess
        {
            get { return _derativeTablesGlobalAccess; }
            set { SetPropertyAndValidate( ref _derativeTablesGlobalAccess, value); }
        }

        [Required]
        public bool DerativeTablesGlobalCascade
        {
            get { return _derativeTablesGlobalCascade; }
            set { SetPropertyAndValidate(ref _derativeTablesGlobalCascade,value); }
        }

        public TableModifierModel()
        {
            //Pour que le binding puisse créer une nouvelle ligne
        }

        public TableModifierModel(TableMetadata defaultSchema)
        {
            //_name = defaultSchema.Name;
            //_isStatic = defaultSchema.IsStatic;

            //_foreignKeys = new ObservableCollection<ForeignKeyModifierModel>();
            //foreach (var fk in defaultSchema.ForeignKeys)
            //    _foreignKeys.Add(new ForeignKeyModifierModel(fk));

            //_derivativeTables = new ObservableCollection<DerivativeTableModifierModel>();
            //foreach (var dt in defaultSchema.DerivativeTables)
            //    _derivativeTables.Add(new DerivativeTableModifierModel(dt));

            //_dataBuilders = new ObservableCollection<DataBuilderModel>();
            //foreach (var col in defaultSchema.ColumnsDefinition)
            //    _dataBuilders.Add(new DataBuilderModel(col));
        }
    }
}
