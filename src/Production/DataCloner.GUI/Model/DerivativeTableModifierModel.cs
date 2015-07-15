using DataCloner.DataClasse.Cache;
using DataCloner.GUI.Framework;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class DerivativeTableModifierModel : ValidatableModel
    {
        private short _serverId;
        private string _database;
        private string _schema;
        private string _table;
        private DerivativeTableAccess _access;
        private bool _cascade;
        private bool _isDeleted;

        [Required]
        public short ServerId
        {
            get { return _serverId; }
            set { SetPropertyAndValidate(ref _serverId, value); }
        }

        [Required]
        public string Database
        {
            get { return _database; }
            set { SetPropertyAndValidate(ref _database, value); }
        }

        [Required]
        public string Schema
        {
            get { return _schema; }
            set { SetPropertyAndValidate(ref _schema, value); }
        }

        [Required]
        public string Table
        {
            get { return _table; }
            set { SetPropertyAndValidate(ref _table, value); }
        }

        [Required]
        public DerivativeTableAccess Access
        {
            get { return _access; }
            set { SetPropertyAndValidate(ref _access, value); }
        }

        [Required]
        public bool Cascade
        {
            get { return _cascade; }
            set { SetPropertyAndValidate(ref _cascade, value); }
        }

        [Required]
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { SetPropertyAndValidate(ref _isDeleted, value); }
        }

        public DerivativeTableModifierModel()
        {
            //Pour que le binding puisse créer une nouvelle ligne
        }

        public DerivativeTableModifierModel(IDerivativeTable derivativeTable)
        {
            _serverId = derivativeTable.ServerId;
            _database = derivativeTable.Database;
            _schema = derivativeTable.Schema;
            _table = derivativeTable.Table;
            _access = derivativeTable.Access;
            _cascade = derivativeTable.Cascade;
        }
    }
}
