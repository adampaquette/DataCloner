using DataCloner.Metadata;
using DataCloner.GUI.Framework;
using System.ComponentModel.DataAnnotations;


namespace DataCloner.GUI.Model
{
    class ForeignKeyColumnModifierModel : ValidatableModelBase
    {
        internal string _nameFrom;
        internal string _nameTo;

        private bool _isDeleted;


        [Required]
        public string NameFrom
        {
            get { return _nameFrom; }
            set { SetPropertyAndValidate(ref _nameFrom, value); }
        }

        [Required]
        public string NameTo
        {
            get { return _nameTo; }
            set { SetPropertyAndValidate(ref _nameTo, value); }
        }

        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { SetProperty(ref _isDeleted, value); }
        }

        public ForeignKeyColumnModifierModel()
        {
            //Pour que le binding puisse créer une nouvelle ligne
        }

        public ForeignKeyColumnModifierModel(ForeignKeyColumn column)
        {
            _nameFrom = column.NameFrom;
            _nameTo = column.NameTo;
        }
    }
}
