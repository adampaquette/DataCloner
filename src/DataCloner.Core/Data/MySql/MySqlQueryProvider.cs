using DataCloner.Core.Data.Generator;
using DataCloner.Core.Data.Generator.MySql;

namespace DataCloner.Core.Data.MySql
{
    internal sealed class QueryHelperMySql : QueryProvider 
    {
        private static QueryHelperMySql _instance;

        public const string ProviderName = "MySql.Data.MySqlClient";

         protected override string SqlGetLastInsertedPk =>
            "SELECT LAST_INSERT_ID();"; 

        protected override string SqlEnforceIntegrityCheck =>
            "SET UNIQUE_CHECKS=@ACTIVE; SET FOREIGN_KEY_CHECKS=@ACTIVE;"; 

        public override DbEngine Engine => DbEngine.MySql;
        public override ISqlTypeConverter TypeConverter { get; }
        public override ISqlWriter SqlWriter { get; }

        public static QueryHelperMySql Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new QueryHelperMySql();
                return _instance;
            }
        }

        public QueryHelperMySql()
        {
            TypeConverter = new MySqlTypeConverter();
            SqlWriter = new MySqlWriter();
        }      
    }
}