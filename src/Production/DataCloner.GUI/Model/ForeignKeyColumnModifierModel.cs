using DataCloner.DataClasse.Cache;
using DataCloner.GUI.Framework;
using System.ComponentModel.DataAnnotations;


namespace DataCloner.GUI.Model
{
    class ForeignKeyColumnModifierModel : ValidatableModel
    {
        private string _nameFrom;
        private string _nameTo;
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

        public ForeignKeyColumnModifierModel(IForeignKeyColumn column)
        {
            _nameFrom = column.NameFrom;
            _nameTo = column.NameTo;
        }
    }
}
