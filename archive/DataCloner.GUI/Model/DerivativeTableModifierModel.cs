using DataCloner.Core;
using DataCloner.GUI.Framework;
using System.ComponentModel.DataAnnotations;

namespace DataCloner.GUI.Model
{
    class DerivativeTableModifierModel : ValidatableModelBase
    {
        internal string _serverId;
        internal string _database;
        internal string _schema;
        internal string _table;
        internal DerivativeTableAccess _access;
        internal bool _cascade;

        private bool _isDeleted;

        [Required]
        public string ServerId
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
    }
}
