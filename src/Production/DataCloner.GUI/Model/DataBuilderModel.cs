using DataCloner.Metadata;
using DataCloner.GUI.Framework;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class DataBuilderModel : ValidatableModel
    {
        private string _columnsName;
        private string _builderName;

        [Required]
        public string ColumnName
        {
            get { return _columnsName; }
            set { SetPropertyAndValidate(ref _columnsName, value); }
        }

        public string BuilderName
        {
            get { return _builderName; }
            set { SetProperty(ref _builderName, value); }
        }

        public DataBuilderModel(IColumnDefinition col)
        {
            _columnsName = col.Name;
            _builderName = col.BuilderName;
        }
    }
}
